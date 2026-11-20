import { Callout as RspressCallout } from '@rspress/core/theme';
import type { ReactNode } from 'react';

type CalloutProps = {
  children: ReactNode;
  title?: string;
  type?: 'info' | 'warn';
};

export function Callout({ children, title, type = 'info' }: CalloutProps) {
  return (
    <RspressCallout type={type === 'warn' ? 'warning' : 'info'} title={title}>
      {children}
    </RspressCallout>
  );
}

export default Callout;
