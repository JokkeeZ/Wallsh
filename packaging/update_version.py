#!/bin/python3
import os
import argparse
import xml.etree.ElementTree as ET

if __name__ == '__main__':
    parser = argparse.ArgumentParser(description='bump the Wallsh project version number')
    parser.add_argument('--path', type=str, help='path for the .csproj file', default='../src/Wallsh.csproj')
    parser.add_argument('--version', type=str, help='specify new version for the project. (for example: 1.2.3)', default=None)

    args = parser.parse_args()

    if not args.path and not args.version and not os.path.exists(args.path):
        parser.print_usage()
        exit()

    tree = ET.parse(args.path)

    for e in tree.getroot().findall('.//PropertyGroup'):
        version = e.find('Version')
        if version is not None:
            if version.text == args.version:
                print(f'Version is already {version.text} in {args.path}')
                break

            old_version = version.text
            version.text = args.version
            print(f'Updated version to {args.version} from {old_version} in {args.path}')
            break

    tree.write(args.path, encoding='utf-8', xml_declaration=True)
