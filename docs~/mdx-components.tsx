import defaultComponents from 'fumadocs-ui/mdx';
import { mdxComponents } from 'ach-fumadocs-theme';
import type { MDXComponents } from 'mdx/types';
import { Mermaid } from './components/Mermaid';

export function getMDXComponents(components?: MDXComponents): MDXComponents {
  return {
    ...defaultComponents,
    ...mdxComponents,
    Mermaid,
    ...components,
  };
}
