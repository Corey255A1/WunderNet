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

def ByteArrayToInt32(barray):
	return barray[0] + (barray[1]<<8) + (barray[2]<<16) + (barray[3]<<24)		

def CharArrayToInt32(carray):
	return ByteArrayToInt32(bytearray(carray))

def Int32ToCharArray(theInt):
	return "".join([chr(theInt & 0xFF), chr((theInt>>8) & 0xFF), chr((theInt>>16) & 0xFF), chr((theInt>>24) & 0xFF)])
	
class BasePacket:
	def __init__(self):
		self.SenderID = ''
		self.ReceiverID = ''
		self.PacketType = PacketTypes['ONLINE']
	def InitPacket(self,sender,receiver,type):
		self.SenderID = sender
		self.ReceiverID = receiver
		self.PacketType = type
	def InitFromPacket(self,packet):
		offset = WUNDERHEADERLEN
		self.SenderID = packet[offset:offset+32].strip()
		offset = offset+32
		self.ReceiverID = packet[offset:offset+32].strip()
		offset = offset+32
		self.PacketType = CharArrayToInt32(packet[offset:offset+4])
		offset = offset+4
		return offset
	
	def GetBytes(self):
		return WUNDERHEADER + (self.SenderID.ljust(32,' ')) + (self.ReceiverID.ljust(32,' ')) + Int32ToCharArray(self.PacketType)
	
class StringDataPacket(BasePacket):
	def __init__(self):
		BasePacket.__init__(self)
		self.DataSize = 0
		self.Data = ''
	
	def InitPacket(self,sender,receiver,data):
		BasePacket.InitPacket(self,sender,receiver,PacketTypes['DATABLOCK'])
		self.Data = data
		self.DataSize = len(data)
		
	def InitFromPacket(self,packet):
		offset = BasePacket.InitFromPacket(self,packet)
		self.DataSize = CharArrayToInt32(packet[offset:offset+4])
		offset = offset+4
		self.Data = packet[offset:]
		
	def GetBytes(self):
		return BasePacket.GetBytes(self) + Int32ToCharArray(self.DataSize) + self.Data

		
class StandardFeature():
	def __init__(self, name, basetype, iotype):
		self.FeatureName = name
		self.FeatureIOType = FeatureIOTypes = iotype
		self.FeatureBaseType = basetype
		
	def InitFromPacket(self, data, offset):
		self.FeatureName = data[offset:offset+32].strip()
		offset = offset + 32
		self.FeatureIOType = CharArrayToInt32(data[offset:offset+4])
		offset = offset + 4
		self.FeatureBaseType = CharArrayToInt32(data[offset:offset+4])
		offset = offset + 4
		return offset
		
	def GetBytes(self):
		return self.FeatureName.ljust(32,' ') + Int32ToCharArray(self.FeatureIOType) + Int32ToCharArray(self.FeatureBaseType)
		
class DescriptionPacket(BasePacket):
	def __init__(self):
		BasePacket.__init__(self)
		self.FeatureCount = 0
		self.FeatureList = []
	
	def InitPacket(self,sender,receiver,features):
		BasePacket.InitPacket(self,sender,receiver,PacketTypes['DESCRIPTION'])
		self.FeatureCount = len(features)
		self.FeatureList = features
	
	def InitFromPacket(self, packet):
		offset = BasePacket.InitFromPacket(self,packet)
		self.FeatureCount = CharArrayToInt32(packet[offset:offset+4])
		offset = offset + 4
		for i in range(0, self.FeatureCount):
			sf = StandardFeature()
			offset = sf.InitFromPacket(packet,offset)
			self.FeatureList.append(sf)
			
	def GetBytes(self):
		thebytes = BasePacket.GetBytes(self) + Int32ToCharArray(self.FeatureCount)
		for i in range(0, self.FeatureCount):
			thebytes = thebytes + self.FeatureList[i].GetBytes()
			
		return thebytes








	