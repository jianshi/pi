import socket

sock = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
sock.connect(('192.168.0.7', 5556))

msgList = ["a", "b", "c", "d"];
try:
    for msg in msgList:
        print(msg)

    #send login message as first one
    sock.send(bytes("LOGIN" + "\n", "utf-8"))
    reply = sock.recv(4096)
    
    for msg in msgList:    
        sock.send(bytes(msg + "\n", "utf-8"))
        #receive
        reply = sock.recv(4096)
        print(reply)

    #send termination message as last one
    sock.send(bytes("END" + "\n", "utf-8"))
    reply = sock.recv(4096)

finally:
    sock.close()
