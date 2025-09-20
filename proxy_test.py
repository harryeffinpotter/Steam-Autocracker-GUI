import requests
from bs4 import BeautifulSoup

# Thunder proxy - correct format for Python requests
proxies = {
    'http': 'http://Hz38aabA8TX9Wo95Mr-dc-ANY:gzhwSXYYZ1l76Xj@gw.thunderproxy.net:5959',
    'https': 'http://Hz38aabA8TX9Wo95Mr-dc-ANY:gzhwSXYYZ1l76Xj@gw.thunderproxy.net:5959'
}

headers = {
    'User-Agent': 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36'
}

# First test if proxy works at all
print("Testing proxy with ip-api.com...")
try:
    test = requests.get('http://ip-api.com/json', proxies=proxies, timeout=10)
    print(f"Proxy IP: {test.json().get('query', 'Unknown')}")
    print(f"Location: {test.json().get('city', 'Unknown')}, {test.json().get('country', 'Unknown')}")
    print("✓ Proxy works!\n")
except Exception as e:
    print(f"Proxy test failed: {e}\n")

# Now try CS.RIN.RU
print("Searching CS.RIN.RU for Barony...")
url = "https://cs.rin.ru/forum/search.php?keywords=Barony&terms=all&author=&sc=1&sf=titleonly&sk=t&sd=d&sr=topics&st=0&ch=300&t=0&submit=Search"

try:
    response = requests.get(url, headers=headers, proxies=proxies, timeout=30, verify=False)
    print(f"Status: {response.status_code}")
    print(f"Size: {len(response.content)} bytes")

    if response.status_code == 200:
        soup = BeautifulSoup(response.content, 'html.parser')

        threads = soup.find_all('a', class_='topictitle')
        print(f"\nFound {len(threads)} threads:")

        for thread in threads:
            print(f"  - {thread.text.strip()}")

        # Check for "cannot use search" message
        if "cannot use search at this time" in response.text:
            print("\n⚠️ Rate limited! CS.RIN.RU says 'cannot use search at this time'")
    else:
        print(f"Failed with status {response.status_code}")

except requests.exceptions.ProxyError as e:
    print(f"Proxy error: {e}")
    print("The proxy credentials might be wrong or expired")

except Exception as e:
    print(f"Error: {e}")