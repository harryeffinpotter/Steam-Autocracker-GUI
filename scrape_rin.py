import requests
from bs4 import BeautifulSoup
import re

# Try to scrape CS.RIN.RU
url = "https://cs.rin.ru/forum/search.php?keywords=Barony&terms=all&author=&sc=1&sf=titleonly&sk=t&sd=d&sr=topics&st=0&ch=300&t=0&submit=Search"

headers = {
    'User-Agent': 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36',
    'Accept': 'text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8',
    'Accept-Language': 'en-US,en;q=0.5',
    'Accept-Encoding': 'gzip, deflate, br',
    'DNT': '1',
    'Connection': 'keep-alive',
    'Upgrade-Insecure-Requests': '1'
}

print("Fetching CS.RIN.RU search page...")

try:
    response = requests.get(url, headers=headers)
    print(f"Status: {response.status_code}")
    print(f"Size: {len(response.content)} bytes\n")

    if response.status_code == 200:
        soup = BeautifulSoup(response.content, 'html.parser')

        # Find all thread links
        threads = soup.find_all('a', class_='topictitle')

        print(f"Found {len(threads)} threads:\n")

        for thread in threads:
            title = thread.text.strip()
            href = thread.get('href', '')

            # Check forum ID
            if 'f=10' in href:
                print(f"[MAIN FORUM] {title}")
            elif 'f=22' in href:
                print(f"[CONTENT SHARING] {title}")
            else:
                print(f"[OTHER] {title}")

            print(f"  -> {href}\n")

        # Find all viewtopic links
        all_links = soup.find_all('a', href=re.compile(r'viewtopic\.php'))
        print(f"\nTotal viewtopic links on page: {len(all_links)}")

        # Find highest page number
        page_links = {}
        for link in all_links:
            href = link.get('href', '')
            match = re.search(r't=(\d+).*?start=(\d+)', href)
            if match:
                thread_id = match.group(1)
                start = int(match.group(2))
                if thread_id not in page_links or start > page_links[thread_id]:
                    page_links[thread_id] = start

        if page_links:
            print("\nHighest page starts per thread:")
            for thread_id, start in page_links.items():
                page_num = (start // 15) + 1
                print(f"  Thread {thread_id}: Page {page_num} (start={start})")
    else:
        print(f"Failed with status {response.status_code}")
        print("CS.RIN.RU is probably blocking requests")

except Exception as e:
    print(f"Error: {e}")
    print("\nCS.RIN.RU is blocking automated requests")
    print("Need to use Selenium or get cookies from browser")