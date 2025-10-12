#!/usr/bin/env python3
"""
Find truly unused methods in EnhancedShareWindow.cs
"""

import re

def find_methods(filepath):
    """Find all private/protected methods"""
    with open(filepath, 'r', encoding='utf-8') as f:
        content = f.read()

    # Pattern to match method declarations
    method_pattern = r'^\s*(private|protected)\s+(?:async\s+)?(?:static\s+)?(?:void|Task<?[^>]*>?|string|bool|int|long|List<[^>]+>|Dictionary<[^>]+>|HashSet<[^>]+>)\s+(\w+)\s*\('

    methods = {}
    for match in re.finditer(method_pattern, content, re.MULTILINE):
        method_name = match.group(2)
        line_num = content[:match.start()].count('\n') + 1
        methods[method_name] = {
            'line': line_num,
            'access': match.group(1),
            'declaration': match.group(0).strip()
        }

    return methods, content

def count_usages(method_name, content, declaration_line):
    """Count how many times a method is referenced (excluding declaration)"""
    lines = content.split('\n')
    count = 0
    usage_lines = []

    for i, line in enumerate(lines, 1):
        # Skip the declaration line
        if i == declaration_line:
            continue

        # Look for method calls (method_name followed by '(')
        if re.search(rf'\b{method_name}\s*\(', line):
            count += 1
            usage_lines.append(i)

    return count, usage_lines

def check_designer_file(method_name):
    """Check if method is wired up in Designer file"""
    try:
        with open('/mnt/c/Users/ysg/source/repos/Steam-Autocracker-GUI/EnhancedShareWindow.Designer.cs', 'r', encoding='utf-8') as f:
            designer_content = f.read()
        return method_name in designer_content
    except:
        return False

if __name__ == '__main__':
    filepath = '/mnt/c/Users/ysg/source/repos/Steam-Autocracker-GUI/EnhancedShareWindow.cs'

    methods, content = find_methods(filepath)

    print(f"Found {len(methods)} private/protected methods")
    print("=" * 80)

    unused = []
    potentially_unused = []

    for name, info in sorted(methods.items(), key=lambda x: x[1]['line']):
        usage_count, usage_lines = count_usages(name, content, info['line'])
        in_designer = check_designer_file(name)

        # Event handlers should be checked in designer
        is_event_handler = name.endswith('_Click') or name.endswith('_Load') or \
                          name.endswith('_Tick') or name.endswith('_FormClosed') or \
                          name.endswith('_CellClick') or '_Mouse' in name or \
                          name.endswith('_CellFormatting') or 'SortCompare' in name or \
                          'ColumnHeaderMouseClick' in name

        if usage_count == 0:
            if is_event_handler:
                if not in_designer:
                    unused.append((name, info['line'], 'Event handler NOT wired in Designer'))
                else:
                    print(f"✓ {name:40} (line {info['line']:4}) - Event handler (in Designer)")
            else:
                unused.append((name, info['line'], 'Never called'))
        elif usage_count == 1:
            potentially_unused.append((name, info['line'], usage_lines))
        else:
            print(f"✓ {name:40} (line {info['line']:4}) - Used {usage_count} times")

    print("\n" + "=" * 80)
    print("UNUSED METHODS (safe to remove):")
    print("=" * 80)
    for name, line, reason in unused:
        print(f"✗ {name:40} (line {line:4}) - {reason}")

    if potentially_unused:
        print("\n" + "=" * 80)
        print("POTENTIALLY UNUSED (used only once - check if self-referencing):")
        print("=" * 80)
        for name, line, usage_lines in potentially_unused:
            print(f"? {name:40} (line {line:4}) - Used at lines: {usage_lines}")

    print(f"\n{len(unused)} methods can be safely removed")
