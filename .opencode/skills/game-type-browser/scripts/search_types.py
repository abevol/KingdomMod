#!/usr/bin/env python3
"""
Search types in .NET assemblies

Usage:
    python search_types.py <assembly-path> <search-pattern>

Examples:
    python search_types.py Assembly-CSharp.dll "Map"
    python search_types.py Assembly-CSharp.dll "Player.*Controller"

Note: On Windows, use double backslashes in paths or forward slashes:
    python search_types.py "D:\\path\\to\\assembly.dll" "Pattern"
    python search_types.py "D:/path/to/assembly.dll" "Pattern"
"""

import subprocess
import sys
import re
import os
from pathlib import Path


def normalize_path(path: str) -> str:
    """Normalize path for Windows compatibility"""
    # Convert backslashes to forward slashes (Windows supports both)
    return path.replace('\\', '/')


def search_types(assembly_path: str, pattern: str) -> dict:
    """Search for types matching the pattern in the assembly"""
    # Normalize path
    normalized_path = normalize_path(assembly_path)
    
    if not Path(normalized_path).exists():
        print(f"Error: Assembly file not found: {normalized_path}", file=sys.stderr)
        sys.exit(1)

    # Use shell command with file redirection to capture output
    import tempfile
    with tempfile.NamedTemporaryFile(mode='w', delete=False, suffix='.txt') as f:
        temp_file = f.name
    
    try:
        # Run ilspycmd and redirect output to temp file
        # Note: -l c lists classes (other types: i=interface, s=struct, d=delegate, e=enum)
        cmd = f'ilspycmd -l c "{normalized_path}" > "{temp_file}" 2>&1'
        result = subprocess.run(cmd, shell=True, capture_output=True, text=True)
        
        if result.returncode != 0:
            print(f"Error: ilspycmd failed to execute", file=sys.stderr)
            sys.exit(1)
        
        # Read output from temp file
        with open(temp_file, 'r', encoding='utf-8') as f:
            output = f.read()
    finally:
        # Clean up temp file
        if os.path.exists(temp_file):
            os.unlink(temp_file)

    # Parse and filter types
    lines = output.strip().split('\n')
    types = []
    for line in lines:
        line = line.strip()
        if line:
            # Remove "Class " prefix
            if line.startswith('Class '):
                types.append(line[6:])  # Remove "Class " prefix
            else:
                types.append(line)
    
    # Filter matched types
    pattern_re = re.compile(pattern, re.IGNORECASE)
    matched_types = [t for t in types if t and pattern_re.search(t)]

    return {
        'total': len(types),
        'matched': len(matched_types),
        'types': sorted(matched_types)
    }


def main():
    if len(sys.argv) < 3:
        print("Usage: python search_types.py <assembly-path> <search-pattern>")
        sys.exit(1)

    assembly_path = sys.argv[1]
    pattern = sys.argv[2]

    result = search_types(assembly_path, pattern)

    print(f"## Search Results")
    print(f"- Assembly: `{Path(assembly_path).name}`")
    print(f"- Pattern: `{pattern}`")
    print(f"- Matched: {result['matched']}/{result['total']} types")
    print()

    if result['matched'] > 0:
        print("### Matched Types")
        for type_name in result['types']:
            print(f"- `{type_name}`")
    else:
        print("*No matching types found*")


if __name__ == "__main__":
    main()
