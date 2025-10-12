#!/usr/bin/env python3
"""
Test generate_game_infos.exe without Steam API key
For PEAK game (AppID: 3527290)
"""

import subprocess
import os
import sys
import time
import json
from pathlib import Path

APPID = "3527290"  # PEAK
GAME_NAME = "PEAK"

def test_generate_without_api():
    """Test generate_game_infos without -s flag (no API key)"""

    exe_path = os.path.join(os.getcwd(), "_bin", "generate_game_infos.exe")
    output_dir = os.path.join(os.getcwd(), "test_output", GAME_NAME)

    # Check if exe exists
    if not os.path.exists(exe_path):
        print(f"❌ generate_game_infos.exe not found at: {exe_path}")
        return False

    # Create output directory
    os.makedirs(output_dir, exist_ok=True)

    print(f"🔧 Testing generate_game_infos WITHOUT API key for {GAME_NAME} (AppID: {APPID})")
    print(f"📁 Output directory: {output_dir}")
    print("-" * 60)

    # Run without -s flag (no Steam API key)
    cmd = [exe_path, APPID, f'-o"{output_dir}"']
    cmd_str = ' '.join(cmd)

    print(f"📝 Command: {cmd_str}")
    print(f"⏳ Running... (timeout: 60 seconds)")

    start_time = time.time()

    try:
        result = subprocess.run(
            cmd_str,
            shell=True,
            capture_output=True,
            text=True,
            timeout=60,
            cwd=os.getcwd()
        )

        elapsed = time.time() - start_time
        print(f"⏱️  Completed in {elapsed:.2f} seconds")
        print("-" * 60)

        # Display output
        if result.stdout:
            print("📤 STDOUT:")
            print(result.stdout)

        if result.stderr:
            print("📤 STDERR:")
            print(result.stderr)

        print(f"🔢 Return code: {result.returncode}")
        print("-" * 60)

        # Check what files were generated
        print("📂 Generated files:")
        steam_settings = os.path.join(output_dir, "steam_settings")

        if os.path.exists(steam_settings):
            for root, dirs, files in os.walk(steam_settings):
                for file in files:
                    file_path = os.path.join(root, file)
                    rel_path = os.path.relpath(file_path, output_dir)
                    size = os.path.getsize(file_path)
                    print(f"   ✅ {rel_path} ({size} bytes)")

                    # Check if achievements.json was created
                    if file == "achievements.json":
                        with open(file_path, 'r') as f:
                            try:
                                data = json.load(f)
                                print(f"      → Contains {len(data)} achievements")
                            except:
                                print(f"      → Failed to parse JSON")
        else:
            print("   ❌ No steam_settings directory created")

        print("-" * 60)

        if result.returncode == 0:
            print("✅ SUCCESS: Tool ran without API key!")
            return True
        else:
            print("⚠️  WARNING: Tool returned non-zero exit code")
            return False

    except subprocess.TimeoutExpired:
        print(f"❌ TIMEOUT: Process exceeded 60 seconds")
        return False
    except Exception as e:
        print(f"❌ ERROR: {str(e)}")
        return False

if __name__ == "__main__":
    print("=" * 60)
    print("GENERATE_GAME_INFOS NO-API TEST")
    print("=" * 60)

    success = test_generate_without_api()

    print("=" * 60)
    if success:
        print("🎉 Test completed successfully!")
        print("The tool can work WITHOUT a Steam API key")
    else:
        print("❌ Test failed or had issues")
        print("The tool may require a Steam API key")
    print("=" * 60)