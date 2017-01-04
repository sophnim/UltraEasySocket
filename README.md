# UltraEasySocket
**울트라이지소켓**


It is a Tcp socket library implemented in C #.

C#으로 구현된 Tcp 소켓 라이브러리입니다.

Very very easy to use but high performance socket library.

매우 매우 사용하기 쉽지만 고성능의 소켓 라이브러리입니다.


There are a lot of socket libraries in the world, but it's hard to find anything very easy to use. The Ultra Easy socket is very simple, as the name suggests. It makes it simple for anyone to network programming easily and provides enough explanation and example projects.

닷넷세상에는 수많은 소켓 라이브러리가 있지만 사용하기가 아주 쉬운건 드물더군요. 울트라이지소켓은 그 이름에서 알수 있듯 매우 간단합니다. 누구라도 쉽게 네트워크 프로그래밍을 할 수 있도록 간단하게 만들었고 충분한 설명과 예제 프로젝트를 제공합니다. 

From a general programmer's point of view, the basic requirements for network programming are simple: sending a byte array means that the incoming array is received as it is. If you do this, you can write your network program on top of it by applying your own protocol. The problem is that it is inconceivably difficult to implement this seemingly simple thing without problems. You have to cope with various network exceptions. When you make a multithreaded structure, you have to lock all the code around for synchronization.

일반적인 프로그래머 입장에서 네트워크 프로그래밍에 대한 기본적인 요구사항은 간단합니다: 바이트 배열을 보내면 보낸 배열 단위 그대로 수신되는 겁니다. 이것만 되면 그 위에 자기만의 프로토콜을 적용하여 네트워크 프로그램을 작성하면 되죠. 문제는 이 간단해 보이는 것을 문제 없이구현하기가 만만치 않게 어렵다는 점입니다. 다양한 네트워크 예외 상황에 대응해야 하며 멀티쓰레드 구조로 만들다 보면 동기화를 위해 코드 곳곳에 lock을 걸어야 하죠.   

The Ultra Easy socket is thread-safe and easy to write multithreaded programs, and is implemented in a lock-free structure. Resource pooling reduces the burden of resource creation and deletion, and provides message encryption, making it easy to use for sophisticated socket connections.

울트라이지소켓은 Thread-Safe하여 멀티 쓰레드 프로그램 작성에 용이하며, Lock-Free 구조로 구현되었습니다. 리소스 풀링을 통해 자원 생성 삭제의 부하를 줄였고 메시지 암호화 기능을 제공하여 엄격한 보안이 필요한 소켓 연결에도 쉽게 사용될 수 있습니다.

Ultra-Socket is a high-performance, reliable network solution that is easy to customize and simple to use, but at the production level because it provides the most basic networking capabilities.

울트라이지소켓은 가장 원초적인 네트워크 기능을 제공하므로 커스터마이징 하기에 쉽고, 사용법도 간단하지만 프로덕션 레벨에서 사용할 수 있는 고성능의 신뢰할수 있는 네트워크 솔루션입니다.

Currently only TCP sockets (IPV4) are supported.

현재 TCP 소켓(IPV4)만 지원합니다.

**Usage:**

**사용법:**

**Download source and Complie(Visual Studio Required).** 

파일을 다운로드해서 비주얼 스튜디오로 컴파일 합니다.

The UltraEasySocket.dll file is created by compiling the UltraEasySocket project during the solution. Add this file to your project references, or directly add the five UltraEasySocket project source files (enums.cs, Crypto.cs, SocketResourceManager.cs, SocketSession.cs, UltraEasyTcpSocket.cs) to your project folder Copy and add.

솔루션 중 UltraEasySocket 프로젝트를 컴파일 함으로서 UltraEasySocket.dll 파일이 생성됩니다. 이 파일을 당신의 프로젝트 참조(References)에 추가하거나, 아니면 직접 UltraEasySocket 프로젝트의 소스 파일 5개(enums.cs, Crypto.cs, SocketResourceManager.cs, SocketSession.cs, UltraEasyTcpSocket.cs)를 당신의 프로젝트 폴더에 복사해서 추가합니다.

**namespace.**

UltraEasySocket namespace를 사용함을 명시합니다.

        using UltraEasySocket;


**Creates an UltraEasyTcpSocket instance. Even if you have multiple Listen and Connect, you only need one instance per program.**

UltraEasyTcpSocket 인스턴스를 생성합니다. 여러개의 Listen, Connect를 한다고 해도 이 인스턴스는 프로그램당 1개만 있으면 됩니다. 

        var ultraES = new UltraEasyTcpSocket(new Action<CallbackEventType, object, object>(OnSocketEventCallback));


**You must specify a callback function when you create an instance. The callback function is structured in the following format: The callback parameter eventType tells you which event is the callback. The meaning of the remaining parameters eventFrom, param depends on each eventType.**

인스턴스를 생성할때 콜백함수를 지정해야 합니다. 콜백함수는 다음과 같은 형식으로 구성됩니다. 콜백 파라미터 eventType으로 어떤 이벤트에 대한 콜백인지를 알 수 있습니다. 각각의 eventType에 따라 나머지 파라미터 eventFrom, param이 의미하는 바가 달라집니다.

        public void OnSocketEventCallback(CallbackEventType eventType, object eventFrom, Object param)
        {
                switch (eventType)
                {
                case CallbackEventType.ACCEPT_FAIL: // some AcceptAsync() function fails 
                        // eventFrom : StartListen() return value: listenID
                        // StartListen()호출 이후 Listen 실패한 경우입니다. eventFrom은 StartListen()이 반환한 값(ListenID)입니다.
                        break;

                case CallbackEventType.ACCEPT_SUCCESS: // Accepted new session : Connection Estabilished
                        // eventFrom : StartListen() return value: listenID
                        // param : accepted SocketSession
                        // StartListen() 호출 이후 새로운 연결을 성공적으로 받은 경우입니다.
                        // eventFrom은 StartListen()이 반환한 값(ListenID)입니다. 이 값으로 어떤 Listen Socket이 Accept한 것인지를
                        // 구별할 수 있습니다.
                        // Object 타입인 param은 SocketSession 클래스로 캐스팅해서 사용합니다. 새로 받은 연결의 SocketSession 객체입니다.
                        break;

                case CallbackEventType.CONNECT_FAIL: // TryConnect() failed
                        // eventFrom : TryConnect() returned object: SocketSession
                        // param : (SocketError)
                        // TryConnect() 호출 이후 연결에 실패한 경우입니다.
                        // eventFrom은 TryConnect()가 반환한 SocketSession 클래스입니다. 이 클래스를 서로 비교해서 어떤 TryConnect()에 의한 이벤트인지를 구별할 수 있습니다.
                        // Object 타입인 param을 SocketError로 변환하면 연결 실패시 발생한 소켓 에러 코드를 알 수 있습니다.
                        break;

                case CallbackEventType.CONNECT_SUCCESS: // TryConnect() succeed : Connection Estabilished
                        // eventFrom : TryConnect() returned object: SocketSession
                        // TryConnect() 호출 이후 연결에 성공한 경우입니다.
                        // eventFrom은 TryConnect()가 반환한 SocketSession 클래스입니다. 이 클래스를 서로 비교해서 어떤 TryConnect()에 의한 접속 성공 이벤트인지를 구별할 수 있습니다.
                        break;
      
                case CallbackEventType.SESSION_RECEIVE_DATA: // Session received data
                        // eventFrom : SocketSession that receive data
                        // param : received byte array (byte[])
                        // 연결된 세션이 데이터를 수신한 경우입니다.
                        // fromID는 데이터를 수신한 sessionID입니다.
                        // Object 타입인 param을 byte[] 로 변환하여 수신된 데이터(바이트 배열)을 알 수 있습니다.
                        // ex) 상대편이 100바이트 배열을 20번 나눠서 보내면, 100바이트 배열 하나씩 20번 수신됩니다.
                        break;
      
                case CallbackEventType.SESSION_CLOSED: // Session has been closed
                        // eventFrom : closed SocketSession
                        // 내가 CloseSession()했거나 상대방이 끊었거나 어떤 경우든 세션이 끊어진 경우입니다.
                        break;
                }
        }
        
**If you create an instance and specify a callback function, you're ready to write. Start listening now with the StartListen () function. UltraEasyTcpSocket has no server-client role separation. Multiple Listen and Connect can be performed simultaneously.**

인스턴스를 생성하며 콜백함수를 지정했으면 쓸 준비가 끝난겁니다. 이제 StartListen() 함수를 통해 리스닝을 시작합니다. UltraEasyTcpSocket은 서버-클라이언트 역할 구분이 따로 없습니다. 복수개의 Listen과 Connect를 동시에 수행할 수 있습니다.

        long listenID = ultraES.StartListen(12300, 100);
        
In the example code 12300 is the port number to listen to and 100 is the listen backlog number. The StartListen () function returns a listenID of type long, which is the value that can be compared with the parameter fromID, which is passed when CallbackEventType.ACCEPT_SUCCESS occurs. This allows you to distinguish which StartListen () is the Accepted event when you run multiple StartListen ().

예시 코드의 12300은 Listen할 포트 번호, 100은 listen backlog 숫자입니다. StartListen()함수는 long 타입의 listenID를 반환하는데, 이 listenID는 CallbackEventType.ACCEPT_SUCCESS 발생시에 전달되는 파라미터 fromID와 비교해 볼 수 있는 값입니다. 이를 통해 여러개의 StartListen()을 실행했을때 어느 StartListen()이 Accept한 이벤트인지를 구분할 수 있습니다.

**StartListen () You can try to connect to one port**

StartListen()한 포트로 접속을 시도할 수 있습니다:

        SocketSession session = ultraES.TryConnect("127.0.0.1", 12300);
        
Attempt to connect with TryConnect () function. In the example code, we are connecting to port 12300 with IP address "127.0.0.1". This function returns immediately and returns the SocketSession class. At the point of return, it may not yet be connected. When the connection is complete, the callback function is called.

TryConnect()함수로 접속을 시도합니다. 예시 코드에서는 IP주소 "127.0.0.1"의 포트 12300으로 접속하고 있습니다. 이 함수는 즉각적으로 반환되며 SocketSession 클래스를 반환합니다. 반환시점에서는 아직 연결이 안되었을 수 있습니다. 연결이 완료되면 콜백함수가 호출됩니다.

If the event type in the callback function is CallbackEventType.CONNECT_FAIL, the connection fails.
If the event type in the callback function is CallbackEventType.CONNECT_SUCCESS, the connection is successful. The SocketSession to which the eventFrom parameter of the callback function is attached. You can compare the SocketSession returned at TryConnect () to determine which event is for which TryConnect ().

콜백함수에서의 이벤트타입이 CallbackEventType.CONNECT_FAIL인 경우는 연결에 실패한 경우입니다.
콜백함수에서의 이벤트타입이 CallbackEventType.CONNECT_SUCCESS인 경우는 연결에 성공한 경우입니다. 콜백함수의 파라미터인 eventFrom이 연결된 SocketSession입니다. TryConnect()시 반환된 SocketSession를 비교해서 어떤 TryConnect()에 대한 이벤트인지를 구분할 수 있습니다.

If TryConnect () succeeds and the connection is made, the listener side also executes the ACCEPT_SUCCESS event callback. At this time, the parameter of the callback parameter is the Accepted SocketSession object and data transmission / reception is possible later.

TryConnect()가 성공해서 연결이 이루어지면, 리스너측에서도 ACCEPT_SUCCESS 이벤트 콜백이 실행됩니다. 이때 콜백 파라미터의 param이 Accept된 SocketSession 객체이고 이후로 데이터 송수신이 가능합니다.

**You can send a byte array to a session using the SocketSession associated with the Send () function:**

Send()함수로 연결된 SocketSession를 이용해서 세션에 바이트 배열을 전송할 수 있습니다:

        var bytes = new byte[100];
        // write data in bytes...
        // session is connected(or accepted) SocketSession instance
        ultraES.Send(session, bytes);
        
If Send () succeeds, the session that receives the data calls the callback function. At this time, the callback function parameter eventFrom contains the received byte array value in the SocketSession and param that received the data. Cast the param to (byte []) to get the value.

Send()가 성공하면 데이터를 수신한 세션은 콜백함수를 호출합니다. 이때 콜백함수 파라미터 eventFrom에는 데이터를 수신한 SocketSession, param에는 수신한 바이트 배열값이 들어가 있습니다. param을 (byte[])으로 캐스팅하여 값을 얻어옵니다. 

        case CallbackEventType.SESSION_RECEIVE_DATA: // Session received data
                // eventFrom : SocketSession that receive data
                // param : received byte array (byte[])
                var receivedByteArray = (byte[])param;
                // do something with receivedByteArray...
                break;
        
**The point of the UltraSocket is clear: the session that receives the byte array I receive is still being received.
If you send a 100 byte array, you will get exactly the same 100 byte array. Multiple pieces of data sent individually will not be split or aggregated. The processing of splitting and merging of transmission data, a feature of TCP sockets, is all done within the UltraSocket. Users do not need to worry.**

울트라이지소켓의 지향점은 명확합니다: 내가 보낸 바이트 배열을 수신하는 세션이 그대로 받는다는 것.
만약 100바이트 배열을 보냈다면 정확하게 똑같은 100바이트 배열을 받게 됩니다. 개별적으로 보낸 여러건의 데이터가 나눠지거나 합쳐져서 읽히지는 않습니다. TCP 소켓의 특징인 전송 데이터의 쪼개짐과 합쳐짐에 대한 처리는 모두 울트라이지소켓 내부에서 이루어 집니다. 사용자는 신경쓸 필요가 없습니다.

**If you want to disconnect the connected session, call the CloseSession () function.**

연결된 세션을 끊고 싶으면 CloseSession()함수를 호출합니다.

        ultraES.CloseSession(session);

The callback function is called in the CallbackEventType.SESSION_CLOSED event when the session termination processing is complete after calling CloseSession ().
If I do not call CloseSession () directly, but the other party hangs up, or if the socket is broken due to a problem, the callback function will be called in the CallbackEventType.SESSION_CLOSED event as well.

CloseSession()을 호출한 후에 세션 종료처리가 완료되면 CallbackEventType.SESSION_CLOSED 이벤트로 콜백함수가 호출됩니다.
내가 직접 CloseSession()을 호출하지 않았다고 해도 상대방이 세션을 끊었거나, 소켓에 문제가 발생해서 끊겼다면 마찬가지로 CallbackEventType.SESSION_CLOSED 이벤트로 콜백함수가 호출됩니다.

**To terminate the program, you must call the Terminate () function of UltraEasyTcpSocket. If you do not call this function, the program will not terminate because the thread remains.**

프로그램 종료를 위해서는 UltraEasyTcpSocket의 Terminate() 함수를 호출해야 합니다. 이 함수를 호출하지 않으면 쓰레드가 남아있기 때문에 프로그램이 종료되지 않습니다.

        ultraES.Terminate();


**Several sample projects are included to show you how to use the library. Let me explain one by one.**

라이브러리의 사용법을 보여주기 위해 여러개의 샘플 프로젝트가 포함되어 있습니다. 하나 하나 설명해 보겠습니다.

The UltraEasySocket.Example project is a project that demonstrates the basic operation of an Ultra Easy socket. This project starts listening and tries to connect, and sends a simple message when connected. The session that receives the message prints the message and then closes the session. As a result, session close event callbacks are executed on both sides. Note that both listening and connection attempts are implemented in the same instance.

UltraEasySocket.Example 프로젝트는 울트라이지소켓의 기본적이 작동을 보여주는 프로젝트입니다. 이 프로젝트는 리슨을 시작하고 연결을 시도하며, 연결되면 간단한 메시지를 전송합니다. 메시지를 수신한 세션은 메시지를 출력한 후 세션을 닫습니다. 그에따라 양측에 세션 닫힘 이벤트 콜백이 실행됩니다. 리스닝과 연결시도가 모두 같은 인스턴스에서 구현되었음에 유의하세요.

The UltraEasySocket.ChatServerExample project and the UltraEasySocket.ChatClientExample project are examples of chat servers / clients simply implemented using an UltraSocket.

UltraEasySocket.ChatServerExample 프로젝트와 UltraEasySocket.ChatClientExample 프로젝트는 울트라이지소켓을 이용하여 간단히 구현된 채팅 서버 / 클라이언트 예제입니다. 

The UltraEasySocket.ExtremeTest project is a project that demonstrates the reliability of Ultra Easy Sockets through extreme testing. Connect 1000 consent sessions and 1000 connect sessions, and then send and receive messages sequentially. In the meantime, a string message is sent to all sessions in a separate thread. Sometimes, you will deliberately terminate the session and continue to connect the new session when the termination is complete. In these situations, it checks to see if there are any misreads or missing messages and whether the number of sessions remains constant.

UltraEasySocket.ExtremeTest 프로젝트는 극한의 테스트를 통해 울트라이지소켓의 안정성을 보여주는 프로젝트입니다. 1000개의 accept 세션과 1000개의 connect 세션을 연결한 후 순차적으로 증가하는 메시지를 주고 받습니다. 이 와중에 별도의 쓰레드에서 스트링 메시지를 모든 세션에 전송합니다. 또한 가끔씩 세션을 고의로 접속 종료시키고 접속 종료가 완료되면 새 세션을 연결하는 것을 반복합니다. 이런 상황에서 잘못 읽히거나 누락된 메시지가 있는지 검사하고 세션의 수가 일정하게 유지되는지를 검사합니다.

**Message Encryption**

메시지 암호화 

UltraSockets has its own built-in message encryption. You can set the encryptLevel parameter to 1 or 2 when creating an UltraEasyTcpSocket instance.

울트라소켓은 자체적으로 메시지 암호화 기능을 내장하고 있습니다. UltraEasyTcpSocket 인스턴스를 생성할때 encryptLevel 파라미터를 1 혹은 2로 설정하면 됩니다.

        var ultraES = new UltraEasyTcpSocket(new Action<CallbackEventType, long, object>(OnSocketEventCallback), encryptLevel:1);
        
EncryptLevel 0 (default) does not encrypt and is the fastest.
EncryptLevel 1 provides moderate security and relatively high speed with xor-based encryption.
EncryptLevel 2 provides the slowest but most secure communication method with AES-256 encryption.

encryptLevel 0(기본값)은 암호화하지 않는 상태이며 처리 속도가 가장 빠릅니다. 
encryptLevel 1은 xor 기반 암호화로 적당한 수준의 보안과 비교적 빠른 속도를 제공합니다.
encryptLevel 2는 AES-256 암호화로 속도는 느리지만 가장 안전한 통신 방법을 제공합니다. 

**Any question about this library send Email to sophnim@gmail.com**

라이브러리에 대한 질문이 있으시면 이메일을 보내주세요: sophnim@gmail.com

Thanks.

감사합니다.







