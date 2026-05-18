package com.achengine.debugconsole;

import android.app.Activity;
import android.content.Context;
import android.graphics.Color;
import android.graphics.PixelFormat;
import android.os.Build;
import android.os.Handler;
import android.os.Looper;
import android.text.Spannable;
import android.text.SpannableString;
import android.text.SpannableStringBuilder;
import android.text.style.ForegroundColorSpan;
import android.util.TypedValue;
import android.view.Gravity;
import android.view.MotionEvent;
import android.view.View;
import android.view.ViewGroup;
import android.view.WindowManager;
import android.widget.Button;
import android.widget.FrameLayout;
import android.widget.LinearLayout;
import android.widget.ScrollView;
import android.widget.TextView;

public class AchDebugConsolePlugin {

    private static volatile WindowManager sWindowManager;
    private static volatile View         sRootView;
    private static volatile TextView     sLogTextView;
    private static volatile ScrollView  sScrollView;
    private static final Handler        sHandler = new Handler(Looper.getMainLooper());

    // Log entry color constants
    private static final int COLOR_ERROR   = Color.parseColor("#FF5555");
    private static final int COLOR_WARNING = Color.parseColor("#FFFF55");
    private static final int COLOR_LOG     = Color.WHITE;

    // Drag tracking for window repositioning
    private static int sInitialX, sInitialY;
    private static float sInitialTouchX, sInitialTouchY;

    /**
     * Shows the floating debug console overlay.
     * Must be called from the Unity thread; actual UI creation runs on the main thread.
     *
     * @param activity The current Unity Activity.
     */
    public static void show(final Activity activity) {
        sHandler.post(() -> {
            if (sRootView != null) return; // 이미 표시 중

            sWindowManager = (WindowManager) activity.getSystemService(Context.WINDOW_SERVICE);

            // WindowManager 레이아웃 파라미터 설정
            int type = Build.VERSION.SDK_INT >= Build.VERSION_CODES.O
                    ? WindowManager.LayoutParams.TYPE_APPLICATION_OVERLAY
                    : WindowManager.LayoutParams.TYPE_PHONE;

            WindowManager.LayoutParams params = new WindowManager.LayoutParams(
                    WindowManager.LayoutParams.MATCH_PARENT,
                    WindowManager.LayoutParams.WRAP_CONTENT,
                    type,
                    WindowManager.LayoutParams.FLAG_NOT_FOCUSABLE,
                    PixelFormat.TRANSLUCENT
            );
            params.gravity = Gravity.TOP | Gravity.START;
            params.x = 0;
            params.y = 0;
            params.width  = (int) (activity.getResources().getDisplayMetrics().widthPixels * 0.95f);
            params.height = (int) (activity.getResources().getDisplayMetrics().heightPixels * 0.5f);

            // 루트 컨테이너 (반투명 배경)
            FrameLayout root = new FrameLayout(activity);
            root.setBackgroundColor(Color.argb(220, 20, 20, 20));

            // 수직 LinearLayout
            LinearLayout layout = new LinearLayout(activity);
            layout.setOrientation(LinearLayout.VERTICAL);
            layout.setLayoutParams(new FrameLayout.LayoutParams(
                    ViewGroup.LayoutParams.MATCH_PARENT,
                    ViewGroup.LayoutParams.MATCH_PARENT
            ));

            // ── 드래그 핸들 / 툴바 ──────────────────────────────
            LinearLayout toolbar = new LinearLayout(activity);
            toolbar.setOrientation(LinearLayout.HORIZONTAL);
            toolbar.setBackgroundColor(Color.argb(255, 40, 40, 40));
            toolbar.setPadding(dp(activity, 8), dp(activity, 4), dp(activity, 8), dp(activity, 4));

            TextView title = new TextView(activity);
            title.setText("AchDebugConsole");
            title.setTextColor(Color.WHITE);
            title.setTextSize(TypedValue.COMPLEX_UNIT_SP, 13f);
            LinearLayout.LayoutParams titleParams = new LinearLayout.LayoutParams(0,
                    ViewGroup.LayoutParams.WRAP_CONTENT, 1f);
            title.setLayoutParams(titleParams);
            toolbar.addView(title);

            Button clearBtn = new Button(activity);
            clearBtn.setText("Clear");
            clearBtn.setTextSize(TypedValue.COMPLEX_UNIT_SP, 11f);
            clearBtn.setPadding(dp(activity, 8), 0, dp(activity, 8), 0);
            clearBtn.setOnClickListener(v -> clear());
            toolbar.addView(clearBtn);

            Button closeBtn = new Button(activity);
            closeBtn.setText("X");
            closeBtn.setTextSize(TypedValue.COMPLEX_UNIT_SP, 11f);
            closeBtn.setPadding(dp(activity, 8), 0, dp(activity, 8), 0);
            closeBtn.setOnClickListener(v -> hide());
            toolbar.addView(closeBtn);

            // 드래그로 창 이동
            final WindowManager.LayoutParams wParams = params;
            toolbar.setOnTouchListener((v, event) -> {
                switch (event.getAction()) {
                    case MotionEvent.ACTION_DOWN:
                        sInitialX = wParams.x;
                        sInitialY = wParams.y;
                        sInitialTouchX = event.getRawX();
                        sInitialTouchY = event.getRawY();
                        return true;
                    case MotionEvent.ACTION_MOVE:
                        wParams.x = sInitialX + (int)(event.getRawX() - sInitialTouchX);
                        wParams.y = sInitialY + (int)(event.getRawY() - sInitialTouchY);
                        if (sWindowManager != null && sRootView != null)
                            sWindowManager.updateViewLayout(sRootView, wParams);
                        return true;
                }
                return false;
            });

            layout.addView(toolbar);

            // ── 로그 스크롤 뷰 ──────────────────────────────────
            ScrollView scrollView = new ScrollView(activity);
            scrollView.setLayoutParams(new LinearLayout.LayoutParams(
                    ViewGroup.LayoutParams.MATCH_PARENT,
                    ViewGroup.LayoutParams.MATCH_PARENT
            ));
            scrollView.setFillViewport(true);

            TextView logText = new TextView(activity);
            logText.setLayoutParams(new ScrollView.LayoutParams(
                    ViewGroup.LayoutParams.MATCH_PARENT,
                    ViewGroup.LayoutParams.WRAP_CONTENT
            ));
            logText.setTextSize(TypedValue.COMPLEX_UNIT_SP, 11f);
            logText.setTextColor(Color.WHITE);
            logText.setPadding(dp(activity, 4), dp(activity, 4), dp(activity, 4), dp(activity, 4));
            logText.setTextIsSelectable(true);

            scrollView.addView(logText);
            layout.addView(scrollView);
            root.addView(layout);

            sLogTextView = logText;
            sScrollView  = scrollView;
            sRootView    = root;

            sWindowManager.addView(root, params);
        });
    }

    /**
     * Removes the overlay window.
     */
    public static void hide() {
        sHandler.post(() -> {
            if (sWindowManager != null && sRootView != null) {
                sWindowManager.removeViewImmediate(sRootView);
                sRootView    = null;
                sLogTextView = null;
                sScrollView  = null;
            }
        });
    }

    /**
     * Appends a log entry to the console.
     * Thread-safe: delegates to the main thread.
     *
     * @param type    LogType string ("Log", "Warning", "Error", "Exception", "Assert")
     * @param message Log message body
     */
    public static void addLog(final String type, final String message) {
        sHandler.post(() -> {
            if (sLogTextView == null) return;

            int color = resolveColor(type);
            String line = "[" + type + "] " + message + "\n";

            SpannableString span = new SpannableString(line);
            span.setSpan(new ForegroundColorSpan(color), 0, line.length(),
                    Spannable.SPAN_EXCLUSIVE_EXCLUSIVE);

            sLogTextView.append(span);

            // 스크롤을 맨 아래로 이동
            if (sScrollView != null) {
                sScrollView.post(() -> sScrollView.fullScroll(ScrollView.FOCUS_DOWN));
            }
        });
    }

    /**
     * Clears all log entries from the console.
     */
    public static void clear() {
        sHandler.post(() -> {
            if (sLogTextView != null) sLogTextView.setText("");
        });
    }

    // ── 내부 헬퍼 ──────────────────────────────────────────────────

    private static int resolveColor(String type) {
        if (type == null) return COLOR_LOG;
        switch (type) {
            case "Error":
            case "Exception":
            case "Assert":
                return COLOR_ERROR;
            case "Warning":
                return COLOR_WARNING;
            default:
                return COLOR_LOG;
        }
    }

    private static int dp(Context ctx, int dp) {
        return (int) TypedValue.applyDimension(
                TypedValue.COMPLEX_UNIT_DIP, dp,
                ctx.getResources().getDisplayMetrics());
    }
}
