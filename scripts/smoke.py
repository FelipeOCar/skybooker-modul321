"""Replay the delivered Postman requests against the running real services. Python 3 standard library only."""
import base64
import datetime
import json
from pathlib import Path
import re
import sys
import urllib.error
import urllib.request
import uuid

root = Path(__file__).resolve().parents[1]
collection = json.loads((root / "postman/SkyBooker.postman_collection.json").read_text(encoding="utf-8"))
variables = {v["key"]: v["value"] for v in collection["variable"]}
run = uuid.uuid4().hex[:10]
now = datetime.datetime.now(datetime.timezone.utc)
variables.update(username="felipe_" + run, email="felipe_" + run + "@example.com", flightNo="SK" + run.upper(),
    departure=(now + datetime.timedelta(days=7)).isoformat(), arrival=(now + datetime.timedelta(days=7, hours=2)).isoformat())

def expand(value):
    return re.sub(r"\{\{(\w+)\}\}", lambda match: str(variables[match[1]]), value)

for item in collection["item"]:
    spec = item["request"]
    headers = {h["key"]: expand(h["value"]) for h in spec["header"]}
    auth = spec["auth"]["type"]
    if auth == "bearer": headers["Authorization"] = "Bearer " + variables["token"]
    if auth == "basic": headers["Authorization"] = "Basic " + base64.b64encode((variables["username"] + ":" + variables["password"]).encode()).decode()
    body = expand(spec["body"]["raw"]).encode() if "body" in spec else None
    request = urllib.request.Request(expand(spec["url"]), data=body, headers=headers, method=spec["method"])
    try:
        response = urllib.request.urlopen(request, timeout=15)
    except urllib.error.HTTPError as error:
        response = error
    except urllib.error.URLError as error:
        print("Dienste nicht erreichbar:", error.reason, file=sys.stderr)
        sys.exit(1)
    with response:
        raw = response.read().decode()
        expected = int(re.search(r"HTTP (\d+)", item["event"][0]["script"]["exec"][0])[1])
        if response.status != expected:
            print(f"FEHLER {item['name']}: erwartet {expected}, erhalten {response.status}. {raw}", file=sys.stderr)
            sys.exit(1)
        for line in item["event"][0]["script"]["exec"][1:]:
            match = re.search(r'pm.collectionVariables.set\("(\w+)", pm.response.json\(\).(\w+)\)', line)
            if match: variables[match[1]] = json.loads(raw)[match[2]]
        print(f"PASS {item['name']}: HTTP {response.status}")
print("20 API-Prüfungen bestanden. Testdaten bleiben für die Live-Demo erhalten.")
