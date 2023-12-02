import os
import argparse
import shutil

# Ensure file location is current directory
os.chdir(os.path.dirname(os.path.abspath(__file__)))

TARGET_ASSEMBLY = 'ModBagman.exe'
LIDGREN_NETWORK = 'Lidgren.Network.dll'
STEAMWORKS_NET = 'Steamworks.NET.dll'
BIN_PATH = '../ModBagman/bin/x86/Debug/net472/'
DEPS_PATH = '../Dependencies/'
ILREPACK_PATH = os.path.abspath('ILRepack.exe')

# Paths to probe for SoG installs
SOG_COMMON_PATHS = [
    'C:/Program Files (x86)/Steam/steamapps/common/SecretsOfGrindea/',
    'D:/Program Files (x86)/Steam/steamapps/common/SecretsOfGrindea/',
    'E:/Program Files (x86)/Steam/steamapps/common/SecretsOfGrindea/',
    'F:/Program Files (x86)/Steam/steamapps/common/SecretsOfGrindea/',
    'C:/SteamLibrary/steamapps/common/SecretsOfGrindea/',
    'D:/SteamLibrary/steamapps/common/SecretsOfGrindea/',
    'E:/SteamLibrary/steamapps/common/SecretsOfGrindea/',
    'F:/SteamLibrary/steamapps/common/SecretsOfGrindea/'
]

# Assemblies that are not merged into the main assembly
ASSEMBLY_MERGE_BLACKLIST = [
    TARGET_ASSEMBLY,
    LIDGREN_NETWORK,
    STEAMWORKS_NET
]

# SoG dependencies
SOG_DEPENDENCIES = [
    'Secrets of Grindea.exe',
    'Secrets of Grindea.pdb',
    'Lidgren.Network.dll',
    'Lidgren.Network.pdb',
    'Steamworks.NET.dll'
]

# XNA dependencies, DLLs only, omit '.dll' part
XNA_DEPENDENCIES = [
    'Microsoft.Xna.Framework',
    'Microsoft.Xna.Framework.Game',
    'Microsoft.Xna.Framework.Graphics',
    'Microsoft.Xna.Framework.Xact',
]

# Search some common paths for XNA install path (usually the GAC)
GAC_COMMON_PATHS = [
    'C:/Windows/Microsoft.NET/assembly/GAC_32/'
]

def merge_assemblies():
    os.chdir(BIN_PATH)
    assemblies = [x for x in os.listdir('./') if x.endswith('.dll') and not 'Xna' in x and not any([True for y in ASSEMBLY_MERGE_BLACKLIST if y in x])]

    # Use ILRepack to join all assemblies into target
    cmd = [
        ILREPACK_PATH,
        '/out:merged/' + TARGET_ASSEMBLY,
        '/zeropekind',
        '/union',
        '/xmldocs',
        TARGET_ASSEMBLY,
        ' '.join(assemblies)
    ]

    os.system(' '.join(cmd))


def install():
    # Search for SoG install path
    for path in SOG_COMMON_PATHS:
        if os.path.exists(path + 'Secrets of Grindea.exe'):
            sog_install_path = path
            break

    if sog_install_path is not None:
        print('Found SoG install path:', sog_install_path)
    else:
        print('Couldn\'t find SoG install path. Skipping...')
        return
        
    output_path = BIN_PATH + 'merged/'

    if sog_install_path is not None:
        try:
            os.mkdir(sog_install_path + 'Mods')
        except:
            pass
        shutil.copyfile(output_path + TARGET_ASSEMBLY, sog_install_path + TARGET_ASSEMBLY)
        print('Installed assembly in SoG directory.')


def fetch_deps():
    sog_install_path = None
    xna_install_path = None

    # Search for SoG install path
    for path in SOG_COMMON_PATHS:
        if os.path.exists(path + SOG_DEPENDENCIES[0]):
            sog_install_path = path
            break

    # Search for XNA in GAC
    for path in GAC_COMMON_PATHS:
        if os.path.exists(path + XNA_DEPENDENCIES[0]):
            xna_install_path = path
            break

    if sog_install_path is not None:
        print('Found SoG install path:', sog_install_path)
    else:
        print('Couldn\'t find SoG install path. Skipping...')

    if xna_install_path is not None:
        print('Found XNA install path:', xna_install_path)
    else:
        print('Couldn\'t find XNA install path. Skipping...')

    if sog_install_path is not None:
        for x in SOG_DEPENDENCIES:
            shutil.copyfile(sog_install_path + x, DEPS_PATH + x)
        print('Copied SoG dependencies!')

    if xna_install_path is not None:
        for x in XNA_DEPENDENCIES:
            base_path = xna_install_path + x + '/'
            dll_path = base_path + [x for x in os.listdir(base_path) if 'v4.0' in x][0] + '/' + x + '.dll'
            shutil.copyfile(dll_path, DEPS_PATH + x + '.dll')
        print('Copied XNA dependencies!')


def parse_args():
    parser = argparse.ArgumentParser(
        prog='ModBagman Utils Script',
        description='Provides utilities for working with ModBagman'
    )

    parser.add_argument('action', choices=['fetch-deps', 'merge', 'install'])

    return parser.parse_args()


if __name__ == '__main__':
    args = parse_args()

    if args.action == 'fetch-deps':
        fetch_deps()
    elif args.action == 'merge':
        merge_assemblies()
    elif args.action == 'install':
        install()
