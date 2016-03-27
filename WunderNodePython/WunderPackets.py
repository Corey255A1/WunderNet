#!/usr/bin/env python
# This is WunderPackets Python Edition

PacketTypes = { 'OFFLINE':     0, 
				'ONLINE':      1, 
				'DISCOVER':    2,
				'IDENTIFY':    3, 
				'DESCRIBE':    4,
				'DESCRIPTION': 5,
				'SUBSCRIBE':   6,
				'DATABLOCK':   7 }
				
FeatureIOTypes   = { 'INPUT':  0, 
					 'OUTPUT': 1, 
					 'INOUT':  2 }
					 
FeatureBaseTypes = { 'ONOFF':  0, 
					 'INTVAL': 1 }

WUNDERHEADER = "!!WunderPacket!!"
WUNDERHEADERLEN = len(WUNDERHEADER)
def CheckHeader(packet):
	if len(packet) >= WUNDERHEADERLEN:
		return packet[:WUNDERHEADERLEN] == WUNDERHEADER
	else:
		return 0
		

class BasePacket:
	_SenderID = ''
	_ReceiverID = ''
	_PacketType = PacketTypes['ONLINE']
	def __init__(self,sender,receiver,type):
		self._SenderID = sender
		self._ReceiverID = receiver
		self._PacketType = type
	def __init__(self,packet):
		offset = WUNDERHEADERLEN
		self._SenderID = packet[offset:offset+32].strip()
		offset = offset+32
		self._ReceiverID = packet[offset:offset+32].strip()
		offset = offset+32
		packetType = bytearray(packet[offset:offset+4])
		self._PacketType = packetType[0] + (packetType[1]<<8) + (packetType[2]<<16) + (packetType[3]<<24)
	
	# def GetBytes(self):
		