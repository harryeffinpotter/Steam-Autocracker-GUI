#!/usr/bin/env python3
"""
Clean up EnhancedShareWindow.cs by:
1. Removing unused methods
2. Removing excessive comments
3. Consolidating code
"""

import re

# Methods to completely remove (never called)
UNUSED_METHODS = [
    'InitializeWindow_OLD',
    'CompressAndUploadFallback',
    'lblHeader_Click',
    'PulseTimer_Tick'
]

def should_remove_comment(line):
    """Check if a comment should be removed"""
    # Keep important comments
    if any(keyword in line.lower() for keyword in ['todo', 'fixme', 'hack', 'bug', 'warning']):
        return False

    # Remove obvious/redundant comments
    redundant_patterns = [
        r'^\s*//\s*(Handle|Check|Get|Set|Create|Update|Show|Hide|Close|Open|Add|Remove)',
        r'^\s*//\s*\w+\s+(button|click|event|handler)',
        r'^\s*//',  # Remove single slashes
        r'^\s*//\s*$',  # Empty comment lines
    ]

    for pattern in redundant_patterns:
        if re.match(pattern, line, re.IGNORECASE):
            return True

    return False

def clean_file(input_path, output_path):
    with open(input_path, 'r', encoding='utf-8') as f:
        lines = f.readlines()

    output_lines = []
    in_unused_method = False
    brace_depth = 0
    skip_next_blank = False

    i = 0
    while i < len(lines):
        line = lines[i]

        # Check if we're entering an unused method
        for method in UNUSED_METHODS:
            if method in line and ('private' in line or 'public' in line or 'protected' in line):
                in_unused_method = True
                brace_depth = 0
                i += 1
                continue

        # Track braces to know when method ends
        if in_unused_method:
            if '{' in line:
                brace_depth += line.count('{')
            if '}' in line:
                brace_depth -= line.count('}')
                if brace_depth <= 0:
                    in_unused_method = False
                    skip_next_blank = True
            i += 1
            continue

        # Skip excessive blank lines after removed methods
        if skip_next_blank and line.strip() == '':
            skip_next_blank = False
            i += 1
            continue
        skip_next_blank = False

        # Remove redundant comments
        if should_remove_comment(line):
            # If next line is blank, skip it too
            if i + 1 < len(lines) and lines[i + 1].strip() == '':
                i += 1
            i += 1
            continue

        # Remove excessive blank lines (more than 2 in a row)
        if line.strip() == '':
            blank_count = 1
            j = i + 1
            while j < len(lines) and lines[j].strip() == '':
                blank_count += 1
                j += 1
            if blank_count > 2:
                # Keep only 2 blank lines
                output_lines.append('\n')
                output_lines.append('\n')
                i = j
                continue

        output_lines.append(line)
        i += 1

    with open(output_path, 'w', encoding='utf-8') as f:
        f.writelines(output_lines)

    return len(lines), len(output_lines)

if __name__ == '__main__':
    input_file = '/mnt/c/Users/ysg/source/repos/Steam-Autocracker-GUI/EnhancedShareWindow.cs'
    output_file = '/mnt/c/Users/ysg/source/repos/Steam-Autocracker-GUI/EnhancedShareWindow_cleaned.cs'

    original_lines, cleaned_lines = clean_file(input_file, output_file)

    print(f"Original: {original_lines} lines")
    print(f"Cleaned: {cleaned_lines} lines")
    print(f"Removed: {original_lines - cleaned_lines} lines ({(original_lines - cleaned_lines) / original_lines * 100:.1f}%)")

    print(f"\nCleaned file saved to: {output_file}")