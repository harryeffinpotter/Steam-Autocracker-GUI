import requests
from bs4 import BeautifulSoup

# YOUR proxy that WORKS
proxy = {
    'http': 'http://Hz38aabA8TX9Wo95Mr-dc-ANY:gzhwSXYYZ1l76Xj@gw.thunderproxy.net:5959',
    'https': 'http://Hz38aabA8TX9Wo95Mr-dc-ANY:gzhwSXYYZ1l76Xj@gw.thunderproxy.net:5959'
}

headers = {
    'User-Agent': 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36'
}

url = "https://cs.rin.ru/forum/search.php?keywords=Barony&terms=all&author=&sc=1&sf=titleonly&sk=t&sd=d&sr=topics&st=0&ch=300&t=0&submit=Search"

print("Using YOUR Thunder proxy to scrape CS.RIN.RU...")

try:
    response = requests.get(url, headers=headers, proxies=proxy, timeout=30)
    print(f"Status: {response.status_code}")

    if response.status_code == 200:
        soup = BeautifulSoup(response.content, 'html.parser')

        # Find thread titles
        threads = soup.find_all('a', class_='topictitle')
        print(f"\nFound {len(threads)} threads:")

        for thread in threads:
            href = thread.get('href', '')
            title = thread.text.strip()

            if 'f=10' in href:
                print(f"\n[MAIN FORUM] {title}")
                print(f"URL: {href}")

                # Extract thread ID for finding pages
                import re
                match = re.search(r't=(\d+)', href)
                if match:
                    thread_id = match.group(1)

                    # Find all page links for this thread
                    page_pattern = f't={thread_id}.*?start=(\\d+)'
                    page_links = re.findall(page_pattern, str(soup))

                    if page_links:
                        max_start = max(int(s) for s in page_links)
                        last_page = (max_start // 15) + 1
                        print(f"Thread has {last_page} pages")
                        print(f"Last page URL: viewtopic.php?f=10&t={thread_id}&start={max_start}")
                    else:
                        print("Single page thread")
    else:
        print(f"Failed with status {response.status_code}")

except Exception as e:
    print(f"Error: {e}")
    print("\nProxy might be dead or misconfigured")