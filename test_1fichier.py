#!/usr/bin/env python3
"""
Test script for 1fichier API upload
Tests uploading files through 1fichier and getting download links
"""

import requests
import os
import json
import sys
from pathlib import Path

# Your 1fichier API key
API_KEY = "PSd6297MACENE2VQD7eNxBWIKrrmTTZb"

def get_upload_server():
    """Get the upload server from 1fichier API"""
    try:
        response = requests.get(
            "https://api.1fichier.com/v1/upload/get_upload_server.cgi",
            headers={
                "Authorization": f"Bearer {API_KEY}",
                "Content-Type": "application/json"
            }
        )

        if response.status_code == 200:
            data = response.json()
            print(f"✓ Got upload server: {data['url']}")
            print(f"  Upload ID: {data['id']}")
            return data
        else:
            print(f"✗ Failed to get upload server: {response.status_code}")
            print(f"  Response: {response.text}")
            return None
    except Exception as e:
        print(f"✗ Error getting upload server: {e}")
        return None

def upload_file(file_path, server_info):
    """Upload a file to 1fichier"""
    try:
        file_path = Path(file_path)
        if not file_path.exists():
            print(f"✗ File not found: {file_path}")
            return None

        file_size = file_path.stat().st_size
        print(f"\n📁 Uploading: {file_path.name}")
        print(f"   Size: {file_size / (1024*1024):.2f} MB")

        # Prepare the upload URL
        upload_url = f"https://{server_info['url']}/upload.cgi?id={server_info['id']}"
        print(f"   URL: {upload_url}")

        # Prepare the file
        with open(file_path, 'rb') as f:
            files = {'file[]': (file_path.name, f, 'application/octet-stream')}

            # Optional parameters
            data = {
                'domain': '0',  # Use 1fichier.com
            }

            # Headers with API key
            headers = {
                'Authorization': f'Bearer {API_KEY}',
            }

            print("   Uploading... (this may take a while)")

            # Make the upload request
            response = requests.post(
                upload_url,
                files=files,
                data=data,
                headers=headers,
                allow_redirects=False
            )

            print(f"   Response status: {response.status_code}")

            if response.status_code == 302:
                # Get redirect location
                location = response.headers.get('Location')
                print(f"✓ Upload complete! Redirected to: {location}")

                # Extract the upload ID from location
                if 'xid=' in location:
                    xid = location.split('xid=')[1].split('&')[0]
                    return {'upload_id': xid, 'server_url': server_info['url']}
                return {'location': location}
            else:
                print(f"✗ Unexpected response: {response.status_code}")
                print(f"   Headers: {response.headers}")
                print(f"   Body: {response.text[:500]}")
                return None

    except Exception as e:
        print(f"✗ Error uploading file: {e}")
        return None

def get_download_links(upload_info):
    """Get the download links after upload"""
    try:
        # Construct the end URL
        end_url = f"https://{upload_info['server_url']}/end.pl"

        params = {
            'xid': upload_info['upload_id']
        }

        headers = {
            'JSON': '1'  # Request JSON response format
        }

        print(f"\n🔗 Getting download links...")
        print(f"   URL: {end_url}?xid={upload_info['upload_id']}")

        response = requests.get(end_url, params=params, headers=headers)

        if response.status_code == 200:
            # Check if response is JSON
            content_type = response.headers.get('content-type', '')
            if 'application/json' in content_type:
                data = response.json()
                print(f"✓ Got links:")
            else:
                # If HTML, try to parse it or show raw
                print(f"✓ Got response (not JSON):")
                print(f"   Content-Type: {content_type}")
                print(f"   Response preview: {response.text[:500]}...")
                return None

            if 'links' in data:
                for link in data['links']:
                    print(f"\n   📥 Download: {link.get('download', 'N/A')}")
                    print(f"   📄 Filename: {link.get('filename', 'N/A')}")
                    print(f"   📏 Size: {link.get('size', 'N/A')} bytes")
                    if 'remove' in link:
                        print(f"   🗑️  Remove: {link.get('remove', 'N/A')}")

                return data['links']
            else:
                print("   No links in response")
                print(f"   Response: {json.dumps(data, indent=2)}")

        else:
            print(f"✗ Failed to get links: {response.status_code}")
            print(f"   Response: {response.text}")

    except Exception as e:
        print(f"✗ Error getting links: {e}")
        return None

def test_small_file():
    """Test with a small file first"""
    print("\n" + "="*50)
    print("1FICHIER API UPLOAD TEST")
    print("="*50)

    # Create a test file
    test_file = Path("test_upload.txt")
    test_content = "This is a test file for 1fichier API upload test.\n" * 100
    test_file.write_text(test_content)

    print(f"\n📝 Created test file: {test_file}")
    print(f"   Size: {len(test_content)} bytes")

    try:
        # Step 1: Get upload server
        server_info = get_upload_server()
        if not server_info:
            print("✗ Failed to get upload server")
            return

        # Step 2: Upload the file
        upload_info = upload_file(test_file, server_info)
        if not upload_info:
            print("✗ Failed to upload file")
            return

        # Step 3: Get download links
        if 'upload_id' in upload_info:
            links = get_download_links(upload_info)
            if links:
                print("\n" + "="*50)
                print("✓ UPLOAD SUCCESSFUL!")
                print("="*50)
                return links[0]['download'] if links else None
        else:
            print("✗ No upload ID received")

    finally:
        # Clean up test file
        if test_file.exists():
            test_file.unlink()
            print(f"\n🧹 Cleaned up test file")

def test_with_file(file_path):
    """Test with a specific file"""
    print("\n" + "="*50)
    print("1FICHIER API UPLOAD TEST - CUSTOM FILE")
    print("="*50)

    # Step 1: Get upload server
    server_info = get_upload_server()
    if not server_info:
        print("✗ Failed to get upload server")
        return

    # Step 2: Upload the file
    upload_info = upload_file(file_path, server_info)
    if not upload_info:
        print("✗ Failed to upload file")
        return

    # Step 3: Get download links
    if 'upload_id' in upload_info:
        links = get_download_links(upload_info)
        if links:
            print("\n" + "="*50)
            print("✓ UPLOAD SUCCESSFUL!")
            print("="*50)
            return links[0]['download'] if links else None
    else:
        print("✗ No upload ID received")

if __name__ == "__main__":
    if len(sys.argv) > 1:
        # Test with provided file
        file_path = sys.argv[1]
        download_url = test_with_file(file_path)
    else:
        # Test with small generated file
        download_url = test_small_file()

    if download_url:
        print(f"\n🎉 Final download URL: {download_url}")
        print("\nYou can now use this URL in your application!")