#import <UIKit/UIKit.h>
#import <Foundation/Foundation.h>

// ─────────────────────────────────────────────────────────────────────────────
// AchDebugConsoleEntry — 단일 로그 항목 모델
// ─────────────────────────────────────────────────────────────────────────────
@interface AchDebugConsoleEntry : NSObject
@property (nonatomic, copy) NSString *type;
@property (nonatomic, copy) NSString *message;
- (instancetype)initWithType:(NSString *)type message:(NSString *)message;
@end

@implementation AchDebugConsoleEntry
- (instancetype)initWithType:(NSString *)type message:(NSString *)message {
    self = [super init];
    if (self) {
        _type    = [type copy];
        _message = [message copy];
    }
    return self;
}
@end

// ─────────────────────────────────────────────────────────────────────────────
// AchDebugConsoleViewController — UITableView로 로그를 표시하는 컨트롤러
// ─────────────────────────────────────────────────────────────────────────────
static NSString * const kCellId = @"AchLogCell";
static const NSInteger   kMaxEntries = 500;

@interface AchDebugConsoleViewController : UIViewController <UITableViewDataSource, UITableViewDelegate>
@property (nonatomic, strong) UITableView  *tableView;
@property (nonatomic, strong) NSMutableArray<AchDebugConsoleEntry *> *entries;
@property (nonatomic, strong) UIView       *dragHandle;
- (void)addLogWithType:(NSString *)type message:(NSString *)message;
- (void)clearLogs;
@end

@implementation AchDebugConsoleViewController

- (void)viewDidLoad {
    [super viewDidLoad];
    self.entries = [NSMutableArray arrayWithCapacity:kMaxEntries];

    self.view.backgroundColor = [UIColor colorWithRed:0.08f green:0.08f blue:0.08f alpha:0.92f];
    self.view.layer.cornerRadius = 8.0f;
    self.view.clipsToBounds = YES;

    // ── 드래그 핸들 바 ────────────────────────────────────────────
    UIView *handle = [[UIView alloc] init];
    handle.backgroundColor = [UIColor colorWithRed:0.2f green:0.2f blue:0.2f alpha:1.0f];
    handle.translatesAutoresizingMaskIntoConstraints = NO;
    [self.view addSubview:handle];
    self.dragHandle = handle;

    // 드래그 핸들 — 제목 레이블
    UILabel *titleLabel = [[UILabel alloc] init];
    titleLabel.text = @"AchDebugConsole";
    titleLabel.textColor = [UIColor whiteColor];
    titleLabel.font = [UIFont boldSystemFontOfSize:13.0f];
    titleLabel.translatesAutoresizingMaskIntoConstraints = NO;
    [handle addSubview:titleLabel];

    // Clear 버튼
    UIButton *clearBtn = [UIButton buttonWithType:UIButtonTypeSystem];
    [clearBtn setTitle:@"Clear" forState:UIControlStateNormal];
    [clearBtn setTitleColor:[UIColor whiteColor] forState:UIControlStateNormal];
    clearBtn.titleLabel.font = [UIFont systemFontOfSize:13.0f];
    clearBtn.translatesAutoresizingMaskIntoConstraints = NO;
    [clearBtn addTarget:self action:@selector(onClearTapped) forControlEvents:UIControlEventTouchUpInside];
    [handle addSubview:clearBtn];

    // Close (X) 버튼
    UIButton *closeBtn = [UIButton buttonWithType:UIButtonTypeSystem];
    [closeBtn setTitle:@"✕" forState:UIControlStateNormal];
    [closeBtn setTitleColor:[UIColor whiteColor] forState:UIControlStateNormal];
    closeBtn.titleLabel.font = [UIFont systemFontOfSize:16.0f];
    closeBtn.translatesAutoresizingMaskIntoRequiresForEachSubview:NO;
    closeBtn.translatesAutoresizingMaskIntoConstraints = NO;
    [closeBtn addTarget:self action:@selector(onCloseTapped) forControlEvents:UIControlEventTouchUpInside];
    [handle addSubview:closeBtn];

    // ── 로그 TableView ────────────────────────────────────────────
    UITableView *tableView = [[UITableView alloc] initWithFrame:CGRectZero style:UITableViewStylePlain];
    tableView.dataSource = self;
    tableView.delegate   = self;
    tableView.backgroundColor = [UIColor clearColor];
    tableView.separatorColor  = [UIColor colorWithWhite:0.3f alpha:0.5f];
    tableView.rowHeight = UITableViewAutomaticDimension;
    tableView.estimatedRowHeight = 30.0f;
    tableView.translatesAutoresizingMaskIntoConstraints = NO;
    [tableView registerClass:[UITableViewCell class] forCellReuseIdentifier:kCellId];
    [self.view addSubview:tableView];
    self.tableView = tableView;

    // ── Auto Layout 제약 ──────────────────────────────────────────
    [NSLayoutConstraint activateConstraints:@[
        // 핸들 바
        [handle.topAnchor constraintEqualToAnchor:self.view.topAnchor],
        [handle.leadingAnchor constraintEqualToAnchor:self.view.leadingAnchor],
        [handle.trailingAnchor constraintEqualToAnchor:self.view.trailingAnchor],
        [handle.heightAnchor constraintEqualToConstant:40.0f],

        // 제목 레이블
        [titleLabel.leadingAnchor constraintEqualToAnchor:handle.leadingAnchor constant:12.0f],
        [titleLabel.centerYAnchor constraintEqualToAnchor:handle.centerYAnchor],

        // Clear 버튼
        [clearBtn.trailingAnchor constraintEqualToAnchor:closeBtn.leadingAnchor constant:-8.0f],
        [clearBtn.centerYAnchor constraintEqualToAnchor:handle.centerYAnchor],

        // Close 버튼
        [closeBtn.trailingAnchor constraintEqualToAnchor:handle.trailingAnchor constant:-12.0f],
        [closeBtn.centerYAnchor constraintEqualToAnchor:handle.centerYAnchor],
        [closeBtn.widthAnchor constraintEqualToConstant:36.0f],

        // TableView
        [tableView.topAnchor constraintEqualToAnchor:handle.bottomAnchor],
        [tableView.leadingAnchor constraintEqualToAnchor:self.view.leadingAnchor],
        [tableView.trailingAnchor constraintEqualToAnchor:self.view.trailingAnchor],
        [tableView.bottomAnchor constraintEqualToAnchor:self.view.bottomAnchor],
    ]];

    // 드래그 제스처 등록
    UIPanGestureRecognizer *pan = [[UIPanGestureRecognizer alloc]
                                    initWithTarget:self action:@selector(onDragHandle:)];
    [handle addGestureRecognizer:pan];
}

// ── 로그 추가 ─────────────────────────────────────────────────────────────────

- (void)addLogWithType:(NSString *)type message:(NSString *)message {
    // 원형 버퍼: 최대 항목 수 초과 시 가장 오래된 항목 제거
    if (self.entries.count >= kMaxEntries)
        [self.entries removeObjectAtIndex:0];

    AchDebugConsoleEntry *entry = [[AchDebugConsoleEntry alloc] initWithType:type message:message];
    [self.entries addObject:entry];
    [self.tableView reloadData];

    // 맨 아래 행으로 스크롤
    if (self.entries.count > 0) {
        NSIndexPath *last = [NSIndexPath indexPathForRow:(NSInteger)self.entries.count - 1 inSection:0];
        [self.tableView scrollToRowAtIndexPath:last atScrollPosition:UITableViewScrollPositionBottom animated:NO];
    }
}

// ── 로그 지우기 ───────────────────────────────────────────────────────────────

- (void)clearLogs {
    [self.entries removeAllObjects];
    [self.tableView reloadData];
}

// ── 버튼 액션 ─────────────────────────────────────────────────────────────────

- (void)onClearTapped {
    [self clearLogs];
}

- (void)onCloseTapped {
    // 싱글톤을 통해 Hide 호출
    extern void _AchConsole_Hide(void);
    _AchConsole_Hide();
}

// ── 드래그 핸들 제스처 ────────────────────────────────────────────────────────

- (void)onDragHandle:(UIPanGestureRecognizer *)pan {
    UIView *parentView = self.view.superview;
    if (!parentView) return;

    CGPoint translation = [pan translationInView:parentView];
    CGPoint center = self.view.center;
    self.view.center = CGPointMake(center.x + translation.x, center.y + translation.y);
    [pan setTranslation:CGPointZero inView:parentView];
}

// ── UITableViewDataSource ─────────────────────────────────────────────────────

- (NSInteger)tableView:(UITableView *)tableView numberOfRowsInSection:(NSInteger)section {
    return (NSInteger)self.entries.count;
}

- (UITableViewCell *)tableView:(UITableView *)tableView cellForRowAtIndexPath:(NSIndexPath *)indexPath {
    UITableViewCell *cell = [tableView dequeueReusableCellWithIdentifier:kCellId forIndexPath:indexPath];
    cell.backgroundColor = [UIColor clearColor];
    cell.selectionStyle  = UITableViewCellSelectionStyleNone;

    AchDebugConsoleEntry *entry = self.entries[(NSUInteger)indexPath.row];

    UIColor *color;
    NSString *typeStr = entry.type;
    if ([typeStr isEqualToString:@"Error"] ||
        [typeStr isEqualToString:@"Exception"] ||
        [typeStr isEqualToString:@"Assert"]) {
        color = [UIColor colorWithRed:1.0f green:0.33f blue:0.33f alpha:1.0f]; // 빨간색
    } else if ([typeStr isEqualToString:@"Warning"]) {
        color = [UIColor colorWithRed:1.0f green:1.0f blue:0.33f alpha:1.0f]; // 노란색
    } else {
        color = [UIColor whiteColor]; // 흰색
    }

    NSString *text = [NSString stringWithFormat:@"[%@] %@", typeStr, entry.message];
    cell.textLabel.text          = text;
    cell.textLabel.textColor     = color;
    cell.textLabel.font          = [UIFont monospacedSystemFontOfSize:11.0f weight:UIFontWeightRegular];
    cell.textLabel.numberOfLines = 0;

    return cell;
}

@end

// ─────────────────────────────────────────────────────────────────────────────
// 싱글톤 상태 관리
// ─────────────────────────────────────────────────────────────────────────────
static UIWindow                        *sConsoleWindow     = nil;
static AchDebugConsoleViewController   *sConsoleController = nil;

static void EnsureControllerExists(void) {
    if (sConsoleController) return;
    sConsoleController = [[AchDebugConsoleViewController alloc] init];
}

// ─────────────────────────────────────────────────────────────────────────────
// C 익스포트 함수 (Unity DllImport 대응)
// ─────────────────────────────────────────────────────────────────────────────

extern "C" {

/// Unity에서 콘솔 창을 표시한다.
void _AchConsole_Show(void) {
    dispatch_async(dispatch_get_main_queue(), ^{
        if (sConsoleWindow) {
            sConsoleWindow.hidden = NO;
            return;
        }

        EnsureControllerExists();

        // Unity의 키 윈도우 위에 오버레이 창을 생성한다
        UIWindowScene *scene = nil;
        for (UIScene *s in [UIApplication sharedApplication].connectedScenes) {
            if ([s isKindOfClass:[UIWindowScene class]]) {
                scene = (UIWindowScene *)s;
                break;
            }
        }

        if (scene) {
            sConsoleWindow = [[UIWindow alloc] initWithWindowScene:scene];
        } else {
            sConsoleWindow = [[UIWindow alloc] initWithFrame:[UIScreen mainScreen].bounds];
        }

        // UIWindowLevelAlert보다 높게 설정하여 Unity 뷰 위에 표시
        sConsoleWindow.windowLevel = UIWindowLevelAlert + 100.0f;
        sConsoleWindow.backgroundColor = [UIColor clearColor];

        // 콘솔 뷰 크기: 화면의 45% 높이, 95% 너비
        CGRect screen = [UIScreen mainScreen].bounds;
        CGFloat w = screen.size.width * 0.95f;
        CGFloat h = screen.size.height * 0.45f;
        CGFloat x = (screen.size.width - w) * 0.5f;
        CGFloat y = 40.0f; // 상단 여백

        sConsoleController.view.frame = CGRectMake(x, y, w, h);
        sConsoleWindow.rootViewController = [[UIViewController alloc] init];
        sConsoleWindow.rootViewController.view.backgroundColor = [UIColor clearColor];
        [sConsoleWindow.rootViewController.view addSubview:sConsoleController.view];
        sConsoleController.view.autoresizingMask = UIViewAutoresizingNone;

        [sConsoleWindow makeKeyAndVisible];
    });
}

/// Unity에서 콘솔 창을 숨긴다.
void _AchConsole_Hide(void) {
    dispatch_async(dispatch_get_main_queue(), ^{
        if (!sConsoleWindow) return;
        sConsoleWindow.hidden = YES;
    });
}

/// 로그 항목을 콘솔에 추가한다. 스레드 안전.
void _AchConsole_AddLog(const char *type, const char *message) {
    NSString *typeStr = type    ? [NSString stringWithUTF8String:type]    : @"Log";
    NSString *msgStr  = message ? [NSString stringWithUTF8String:message] : @"";

    dispatch_async(dispatch_get_main_queue(), ^{
        EnsureControllerExists();
        [sConsoleController addLogWithType:typeStr message:msgStr];
    });
}

/// 콘솔의 모든 로그를 지운다.
void _AchConsole_Clear(void) {
    dispatch_async(dispatch_get_main_queue(), ^{
        if (sConsoleController)
            [sConsoleController clearLogs];
    });
}

} // extern "C"
