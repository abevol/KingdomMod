#!/usr/bin/env python3
"""
Extract type members (methods, properties, fields, etc.) from .NET assemblies

Usage:
    python get_type_members.py <assembly-path> <type-name>

Examples:
    python get_type_members.py Assembly-CSharp.dll "PlayerController"
"""

import subprocess
import sys
import re
import os
from pathlib import Path


def normalize_path(path: str) -> str:
    """Normalize path for Windows compatibility"""
    return path.replace('\\', '/')


def get_type_members(assembly_path: str, type_name: str) -> str:
    """Get type member information"""
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


def parse_members(code: str) -> dict:
    """Parse code and extract member information"""
    members = {
        'fields': [],
        'properties': [],
        'methods': [],
        'events': [],
        'constructors': []
    }

    lines = code.split('\n')
    for line in lines:
        line = line.strip()
        
        # Skip empty lines, comments, using statements, namespaces, etc.
        if not line or line.startswith('//') or line.startswith('using ') or \
           line.startswith('namespace ') or line.startswith('[') or \
           line == '{' or line == '}':
            continue

        # Fields
        if re.match(r'(public|private|protected|internal)\s+(static\s+)?\w+(\<.*\>)?\s+\w+;', line):
            members['fields'].append(line.rstrip(';'))
        
        # Properties
        elif re.match(r'(public|private|protected|internal)\s+(static\s+)?\w+(\<.*\>)?\s+\w+\s*\{', line):
            members['properties'].append(line.rstrip('{').strip())
        
        # Methods
        elif re.match(r'(public|private|protected|internal)\s+(static\s+)?(\w+(\<.*\>)?)\s+\w+\s*\(', line):
            # Exclude constructors
            if not re.match(r'.+\s+(\w+)\s*\(', line):
                members['methods'].append(line.split('{')[0].strip())
            else:
                method_match = re.match(r'(public|private|protected|internal)\s+(static\s+)?(\w+(\<.*\>)?)\s+(\w+)\s*\(', line)
                if method_match:
                    return_type = method_match.group(3)
                    method_name = method_match.group(5)
                    # Constructor: method name matches class name and return type
                    if return_type == method_name:
                        members['constructors'].append(line.split('{')[0].strip())
                    else:
                        members['methods'].append(line.split('{')[0].strip())

        # Events
        elif 'event ' in line:
            members['events'].append(line.rstrip(';'))

    return members


def main():
    if len(sys.argv) < 3:
        print("Usage: python get_type_members.py <assembly-path> <type-name>")
        sys.exit(1)

    assembly_path = sys.argv[1]
    type_name = sys.argv[2]

    code = get_type_members(assembly_path, type_name)
    members = parse_members(code)

    print(f"## Type Members: `{type_name}`")
    print(f"Assembly: `{Path(assembly_path).name}`")
    print()

    # Constructors
    if members['constructors']:
        print("### Constructors")
        for ctor in members['constructors']:
            print(f"- `{ctor}`")
        print()

    # Fields
    if members['fields']:
        print("### Fields")
        for field in members['fields']:
            print(f"- `{field}`")
        print()

    # Properties
    if members['properties']:
        print("### Properties")
        for prop in members['properties']:
            print(f"- `{prop}`")
        print()

    # Methods
    if members['methods']:
        print("### Methods")
        for method in members['methods']:
            print(f"- `{method}`")
        print()

    # Events
    if members['events']:
        print("### Events")
        for event in members['events']:
            print(f"- `{event}`")
        print()

    if not any(members.values()):
        print("*No public members found*")


if __name__ == "__main__":
    main()
