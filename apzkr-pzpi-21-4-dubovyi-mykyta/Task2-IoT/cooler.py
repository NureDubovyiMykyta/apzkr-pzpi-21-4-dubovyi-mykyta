import socket

HOST = 'localhost'
PORT = 65433

def handle_command(command):
    if command == "turn_on":
        response = "Cooler is turned ON"
        print(response)
    elif command == "turn_off":
        response = "Cooler is turned OFF"
        print(response)
    else:
        response = f"Unknown command: {command}"
        print(response)
    return response

if __name__ == "__main__":
    with socket.socket(socket.AF_INET, socket.SOCK_STREAM) as s:
        s.bind((HOST, PORT))
        s.listen()
        print(f"Cooler listening on {HOST}:{PORT}")
        while True:
            conn, addr = s.accept()
            with conn:
                print('Connected by', addr)
                while True:
                    data = conn.recv(1024)
                    if not data:
                        break
                    command = data.decode('utf-8')
                    response = handle_command(command)
                    conn.sendall(response.encode('utf-8'))
