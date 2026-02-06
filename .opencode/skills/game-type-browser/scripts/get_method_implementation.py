#!/usr/bin/env python3
"""
Extract .NET method implementation code

Usage:
    python get_method_implementation.py <assembly-path> <type-name>

Examples:
    python get_method_implementation.py Assembly-CSharp.dll "PlayerController"
    
Note: This script decompiles the entire type. AI can extract specific method implementations from the output.
"""

import subprocess
import sys
import os
from pathlib import Path


def normalize_path(path: str) -> str:
    """Normalize path for Windows compatibility"""
    return path.replace('\\', '/')


def get_type_code(assembly_path: str, type_name: str) -> str:
    """Get complete decompiled code for the type"""
    normalized_path = normalize_path(assembly_path)
    
    if not Path(normalized_path).exists():
        print(f"Error: Assembly file not found: {normalized_path}", file=sys.stderr)
        sys.exit(1)

    # Use shell command with file redirection
    import tempfile
    with tempfile.NamedTemporaryFile(mode='w', delete=False, suffix='.txt') as f:
        temp_file = f.name
    
    try:
        cmd = f'ilspycmd -t "{type_name}" "{normalized_path}" > "{temp_file}" 2>&1'
        result = subprocess.run(cmd, shell=True, capture_output=True, text=True)
        
        if result.returncode != 0:
            print(f"Error: ilspycmd failed to execute", file=sys.stderr)
            sys.exit(1)
        
        with open(temp_file, 'r', encoding='utf-8') as f:
            output = f.read()
    finally:
        if os.path.exists(temp_file):
            os.unlink(temp_file)

    return output


def main():
    if len(sys.argv) < 3:
        print("Usage: python get_method_implementation.py <assembly-path> <type-name>")
        sys.exit(1)

    assembly_path = sys.argv[1]
    type_name = sys.argv[2]

    code = get_type_code(assembly_path, type_name)

    print(f"## Type Implementation: `{type_name}`")
    print(f"Assembly: `{Path(assembly_path).name}`")
    print()
    print("```csharp")
    print(code)
    print("```")


if __name__ == "__main__":
    main()
