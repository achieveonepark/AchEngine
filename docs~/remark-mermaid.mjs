function transformMermaid(node) {
  if (!Array.isArray(node.children)) return;

  node.children = node.children.map((child) => {
    if (child.type === 'code' && child.lang === 'mermaid') {
      return {
        type: 'mdxJsxFlowElement',
        name: 'Mermaid',
        attributes: [
          {
            type: 'mdxJsxAttribute',
            name: 'chart',
            value: child.value,
          },
        ],
        children: [],
        position: child.position,
      };
    }

    transformMermaid(child);
    return child;
  });
}

export default function remarkMermaid() {
  return (tree) => transformMermaid(tree);
}
