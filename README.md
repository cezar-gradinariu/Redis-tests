# Redis-tests

I want to use redis -via StackExchange nuget - for caching. Redis is a distributed cache, key-value where the value is a string or a byte[]. That means, that any object I try to put in this cache against a key has to be either string or byte[]. This raises the obvious question, how to transform my complicated objects into a string or an array of bytes?

Well, there are different libraries that will serialize your objects into a string or a byte[] and also provide features to deserialize them back, most of them well-known, some of them totally new to me.

The ones I try here are:

1. Newtonsoft.Json => to json and back
2. JIL => to json and back (https://github.com/kevin-montrose/Jil)
3. NetJson => to json and back
4. NETSerializer => to byte[] and back
5. Bois => byte[] and back
6. MsgPack => byte[] and back

How to use: 

1. Compile solution 
2. Run the exe after and drink a coffee as will take a while (45 min? - depends on you gear, but anyway a lot). 
3. A report will be generated in the bin folder in a file called "report.txt". 

For test I use a list of objects that I serialize, insert in cache, extract from cache and then de-serialize and I try different lengths for that list to emulate my real life scenario. By the way in my real-life project we have flat objects that we store, that have around 15 properties on them and they are stored as list. So, naturally, as this is about me, this test will use lists of different lengths to identify the best serializer library for this particular case.

Also, a very important aspect of this tests is the ratio between writes and reads: Basically, it is used for caching so, it has to write once and be read multiple times, which means that for my tests to be realistic, I have to emulate a case where I write once and read multiple times. This I call the writeReadRatio.

I usually expect this to be around 100 or more.


So, from my tests it results a clear winner across all my scenarios and is Bois library. The downsides with that 

1. It is not fantastically better then the 2nd place - JIL - 
2. It will generate byte[], which is not readable when I look into the key in Redis directly. So trying to debug something in prod is not easy. 

Q & A:
1. 
* Q: Why not Protobuf?
* A: While I never used it before, and while it looks to be very fast, it is also, at least for me very awkward to use due to all the attributes I have to pollute my types with.
2. 
* Q: What did you not like about NETSerializer?
* A: For this scenario is slow and I have to put [Serializable] attributes on the types.. not an option, and easy choice as is slower than BOIS/JIL. Plus I have to register the types I want to serialize.. not cool.
3. 
* Q: What did you not like about MsgPack ?
* A: Slow, byte[]
4. 
* Q: What did you not like about NetJson?
* A: I had so many hopes with this one... It is supposed to be extremely fast. Not in my tests, not by far. It is the slowest of them all. Nothing else to say.
5. 
* Q: What did you not like about NewtonSoft?
* A: Well, speed is good but it is still slower than BOIS/ JIL. Otherwise, absolutely amazing and I use it heavily in my other projects. Will skip on this one though.
6. 
* Q: What did you not like about JIL? (https://github.com/kevin-montrose/Jil)
* A: I like it. Is really good for this scenario, I like that it will get me json on serialization and is fast on output.
7. 
* Q: What did you not like about BOIS? (https://github.com/salarcode/Bois)
* A: Is really fast. But is not json and that makes things hard to debug in production environments. I still think I might use it despite this disadvantage.


For me the nicest surprise was BOIS, which is fast and small in size in this scenario. I suppose because is small in size it is a better candidate than JIL in a distributed environment such as REDIS, as it implies storage and network traffic reduction. I mean is already faster than JIL but also the size might give it another bonus in a real-life scenario.
