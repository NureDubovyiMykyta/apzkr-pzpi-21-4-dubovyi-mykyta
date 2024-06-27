import requests
import time
import random
import json

# Конфігурація
SERVER_URL = "https://localhost:7271/api/Sensor/log-data"  # Замініть на ваш фактичний URL
SENSOR_ID = "667b3b0b092ce839866d19db"  # Замініть на фактичний ID сенсора
INTERVAL = 60  # Інтервал у секундах між відправками даних

def send_data():
    temperature = round(random.uniform(15.0, 30.0), 2)  # Генерація випадкової температури
    data = {
        "sensorId": SENSOR_ID,
        "value": temperature
    }
    headers = {
        "Content-Type": "application/json"
    }
    response = requests.post(SERVER_URL, data=json.dumps(data), headers=headers, verify=False)
    if response.status_code == 200:
        print(f"Data sent: {data}")
    else:
        print(f"Failed to send data: {response.status_code}, {response.text}")

if __name__ == "__main__":
    while True:
        send_data()
        time.sleep(INTERVAL)