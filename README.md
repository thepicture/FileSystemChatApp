# FileSystemChatApp
A simple chat app which I made for a college. It was intended to be used in firewall-secured computes in a localhost. That is, people can chat without the internet and avoid firewall security through app's filesystem sending algorithm.

Algorithm:
FOR EVERY COMPUTER:
  1. When joined to chat, keep sending keep-alive request in a global file.
  2. If send a message, update a global file.
