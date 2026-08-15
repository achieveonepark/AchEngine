'use client';

import { useEffect, useId, useState } from 'react';
import mermaid from 'mermaid';

type MermaidProps = {
  chart: string;
};

let initialized = false;

function initializeMermaid() {
  if (initialized) return;

  mermaid.initialize({
    startOnLoad: false,
    securityLevel: 'strict',
    theme: 'base',
    themeVariables: {
      darkMode: true,
      background: 'transparent',
      primaryColor: '#1e3a5f',
      primaryTextColor: '#e2e8f0',
      primaryBorderColor: '#3b82f6',
      lineColor: '#94a3b8',
      secondaryColor: '#162032',
      tertiaryColor: '#0f2d4a',
    },
  });
  initialized = true;
}

export function Mermaid({ chart }: MermaidProps) {
  const reactId = useId();
  const [svg, setSvg] = useState<string>();
  const [error, setError] = useState<string>();
  const renderId = `mermaid-${reactId.replace(/[^a-zA-Z0-9-_]/g, '')}`;

  useEffect(() => {
    let disposed = false;

    async function render() {
      try {
        initializeMermaid();
        const result = await mermaid.render(renderId, chart);
        if (disposed) return;
        setSvg(result.svg);
        setError(undefined);
      } catch (renderError) {
        if (disposed) return;
        setSvg(undefined);
        setError(renderError instanceof Error ? renderError.message : 'Mermaid 렌더링에 실패했습니다.');
      }
    }

    void render();
    return () => {
      disposed = true;
    };
  }, [chart, renderId]);

  if (error) {
    return (
      <details className="my-4 rounded-lg border border-red-500/40 bg-red-950/20 p-4">
        <summary className="cursor-pointer text-sm text-red-300">Mermaid 렌더링 오류</summary>
        <pre className="mt-3 overflow-x-auto text-xs text-red-200">{chart}</pre>
        <p className="mt-3 text-xs text-red-300">{error}</p>
      </details>
    );
  }

  return (
    <div
      className="my-6 overflow-x-auto rounded-lg border border-fd-border bg-fd-card p-4 [&_svg]:mx-auto [&_svg]:h-auto [&_svg]:max-w-full"
      dangerouslySetInnerHTML={svg ? { __html: svg } : undefined}
      aria-busy={!svg}
    />
  );
}
