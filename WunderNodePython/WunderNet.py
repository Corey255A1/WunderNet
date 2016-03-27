#!/usr/bin/env python
# This is WunderNet Python Edition

import socket
import thread

class WunderNet:
	_bRunServer = 0
	def __init__(self):
		self.udpPort = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
		self._registered_listeners = []
	
	def RegisterCallback(self, function):
		self._registered_listeners.append(function)
	
	def SendPacket(packet):
		self.udpPort.sendto(packet, "192.168.0.255")
		
	def StartListening(self, port):
		if not self._bRunServer:
			self._bRunServer = 1
			self.udpPort.setsockopt(socket.SOL_SOCKET, socket.SO_REUSEADDR, 1)
			self.udpPort.bind(("",port))
			thread.start_new_thread(self.PacketReader,('',''))
	def StopListening(self):
		self._bRunServer = 0
			
	def PacketReader(self,*args):
		while self._bRunServer:
			packet = self.udpPort.recvfrom(2048)
			data = packet[0]
			addr = packet[1]
			
			if not data:
				time.sleep(.1)
				break
				
			for callback in self._registered_listeners:
				thread.start_new_thread(callback,(data,1))
