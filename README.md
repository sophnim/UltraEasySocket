# UltraEasySocket
**울트라이지소켓**


Tcp Socket library for C# 

C#으로 구현된 Tcp 소켓 라이브러리입니다.

Very Very Easy Usage But Built for Hi Perfomance.

매우 매우 사용하기 쉽지만 고성능의 소켓 라이브러리입니다.


In the .Net World, There are many socket libraries... But not so easy to use. 

닷넷세상에는 수많은 소켓 라이브러리가 있지만 사용하기가 아주 쉬운건 드물더군요. 울트라이지소켓은 그 이름에서 알수 있듯 매우 간단합니다. 누구라도 쉽게 네트워크 프로그래밍을 할 수 있도록 간단하게 만들었고 충분한 설명과 예제 프로젝트를 제공합니다. 

Most programmer's networking need is simple: I send byte array and You receive same byte array.

일반적인 프로그래머 입장에서 네트워크 프로그래밍에 대한 기본적인 요구사항은 간단합니다: 바이트 배열을 보내면 보낸 배열 단위 그대로 수신되는 겁니다. 이것만 되면 그 위에 자기만의 프로토콜을 적용하여 네트워크 프로그램을 작성하면 되죠. 문제는 이 간단해 보이는 것을 문제 없이구현하기가 만만치 않게 어렵다는 점입니다. 다양한 네트워크 예외 상황에 대응해야 하며 멀티쓰레드 구조로 만들다 보면 동기화를 위해 코드 곳곳에 lock을 걸어야 하죠.   

UltraEasySocket is Thread-Safe, implemented by Lock-Free algorithm, Resource Pooling, and has internal message encryption!

울트라이지소켓은 Thread-Safe하여 멀티 쓰레드 프로그램 작성에 용이하며, .Net Framework4의 Concurrent 컬렉션을 사용한 Lock-Free 구조로 구현되었습니다. 리소스 풀링을 통해 자원 생성 삭제의 부하를 줄였고 메시지 암호화 기능을 제공하여 엄격한 보안이 필요한 소켓 연결에도 쉽게 사용될 수 있습니다.

This is the simplest but Hi-performance, and reliable solution for socket networking.

울트라이지소켓은 가장 원초적인 네트워크 기능을 제공하므로 커스터마이징 하기에 쉽고, 사용법도 간단하지만 프로덕션 레벨에서 사용할 수 있는 고성능의 신뢰할수 있는 네트워크 솔루션입니다.

In this moment I implemented TCP-socket(IPV4) only.

현재 TCP 소켓(IPV4)만 지원합니다.

**Usage:**

**사용법:**

**Download source and Complie(Visual Studio Required).** 

파일을 다운로드해서 비주얼 스튜디오로 컴파일 합니다.

Add UltraEasySocket.dll to Your project reference or just add UltraEasySocket source files(enums.cs, SocketResourceManager.cs, SocketSession.cs, UltraEasyTcpSocket.cs) to Your project.

솔루션 중 UltraEasySocket 프로젝트를 컴파일 함으로서 UltraEasySocket.dll 파일이 생성됩니다. 이 파일을 당신의 프로젝트 참조(References)에 추가하거나, 아니면 직접 UltraEasySocket 프로젝트의 소스 파일 5개(enums.cs, Crypto.cs, SocketResourceManager.cs, SocketSession.cs, UltraEasyTcpSocket.cs)를 당신의 프로젝트 폴더에 복사해서 추가합니다.

**namespace.**

UltraEasySocket namespace를 사용함을 명시합니다.

        using UltraEasySocket;


**Create UltraEasySocket Instance: Although you are going to create multiple listen or connect session, one instance is enough.**

UltraEasyTcpSocket 인스턴스를 생성합니다. 여러개의 Listen, Connect를 한다고 해도 이 인스턴스는 프로그램당 1개만 있으면 됩니다. 

        var ultraES = new UltraEasyTcpSocket(new Action<CallbackEventType, long, object>(OnSocketEventCallback));


**UltraEasySocket is callback based. Define callback function with this parameters :**

인스턴스를 생성할때 콜백함수를 지정해야 합니다. 콜백함수는 다음과 같은 형식으로 구성됩니다. 콜백 파라미터 eventType으로 어떤 이벤트에 대한 콜백인지를 알 수 있습니다. 각각의 eventType에 따라 나머지 파라미터 fromID, param이 의미하는 바가 달라집니다.

        public void OnSocketEventCallback(CallbackEventType eventType, long fromID, Object param)
        {
                switch (eventType)
                {
                case CallbackEventType.ACCEPT_FAIL: // some AcceptAsync() function fails 
                        // fromID : StartListen() return value: listenID
                        // StartListen()호출 이후 Listen 실패한 경우입니다. fromID는 StartListen()이 반환한 값(ListenID)입니다.
                        break;

                case CallbackEventType.ACCEPT_SUCCESS: // Accepted new session : Connection Estabilished
                        // fromID : StartListen() return value: listenID
                        // param : (long)acceptedSessionID
                        // StartListen() 호출 이후 새로운 연결을 성공적으로 받은 경우입니다.
                        // fromID는 StartListen()이 반환한 값(ListenID)입니다. 이 값으로 어떤 Listen Socket이 Accept한 것인지를
                        // 구별할 수 있습니다.
                        // Object 타입인 param을 long형으로 변환하면 새로 받은 연결의 sessionID를 알 수 있습니다.
                        break;

                case CallbackEventType.CONNECT_FAIL: // TryConnect() failed
                        // fromID : TryConnect() return value: sessionID
                        // param : (SocketError)
                        // TryConnect() 호출 이후 연결에 실패한 경우입니다.
                        // Object 타입인 param을 SocketError로 변환하면 연결 실패시 발생한 소켓 에러 코드를 알 수 있습니다.
                        break;

                case CallbackEventType.CONNECT_SUCCESS: // TryConnect() succeed : Connection Estabilished
                        // fromID : TryConnect() return value: SessionID
                        // TryConnect() 호출 이후 연결에 성공한 경우입니다.
                        // fromID는 TryConnect()가 반환한 값(sessionID)입니다. 이 값으로 어떤 세션이 연결된 것인지를 알 수 있습니다.
                        break;
      
                case CallbackEventType.SESSION_RECEIVE_DATA: // Session received data
                        // fromID : sessionID that receive data
                        // param : received byte array (byte[])
                        // 연결된 세션이 데이터를 수신한 경우입니다.
                        // fromID는 데이터를 수신한 sessionID입니다.
                        // Object 타입인 param을 byte[] 로 변환하여 수신된 데이터(바이트 배열)을 알 수 있습니다.
                        // ex) 상대편이 100바이트 배열을 20번 나눠서 보내면, 100바이트 배열 하나씩 20번 수신됩니다.
                        break;
      
                case CallbackEventType.SESSION_CLOSED: // Session has been closed
                        // fromID : closed sessionID
                        // 내가 CloseSession()했거나 상대방이 끊었거나 어떤 경우든 세션이 끊어진 경우입니다.
                        break;
                }
        }
        
**Ok. UltraEasySocket has no separation between Server-Client. UltraEasyTcpSocket Instance can listen and connect at the same time. Let's start listen:**

인스턴스를 생성하며 콜백함수를 지정했으면 쓸 준비가 끝난겁니다. 이제 StartListen() 함수를 통해 리스닝을 시작합니다. UltraEasyTcpSocket은 서버-클라이언트 역할 구분이 따로 없습니다. 복수개의 Listen과 Connect를 동시에 수행할 수 있습니다.

        long listenID = ultraES.StartListen(12300, 100);
        
12300 means port number to listen, 100 means listen backlog number. Return value is listenID (long). You can use listenID to identify which listen socket is accepted new session in CallbackEventType.ACCEPT_SUCCESS event in callback function.

예시 코드의 12300은 Listen할 포트 번호, 100은 listen backlog 숫자입니다. StartListen()함수는 long 타입의 listenID를 반환하는데, 이 listenID는 CallbackEventType.ACCEPT_SUCCESS 발생시에 전달되는 파라미터 fromID와 비교해 볼 수 있는 값입니다. 이를 통해 여러개의 StartListen()을 실행했을때 어느 StartListen()이 Accept한 이벤트인지를 구분할 수 있습니다.

**Next, Try to connect to listening port:**

StartListen()한 포트로 접속을 시도할 수 있습니다:

        long sessionID = ultraES.TryConnect("127.0.0.1", 12300);
        
TryConnect() function tries to connect to ip address "127.0.0.1" port 12300. It Immediately returns sessionID, but does not means that connect estabilished. If connect succeed or failed, callback will be executed. 

TryConnect()함수로 접속을 시도합니다. 예시 코드에서는 IP주소 "127.0.0.1"의 포트 12300으로 접속하고 있습니다. 이 함수는 즉각적으로 반환되며 long 타입의 sessionID를 반환합니다. 반환시점에서는 아직 연결이 안되었을 수 있습니다. 연결이 완료되면 콜백함수가 호출됩니다.

CallbackEventType.CONNECT_FAIL means that TryConnect() has been failed. 
CallbackEventType.CONNECT_SUCCESS means that TryConnect() has been succeed and You can send data now.

콜백함수에서의 이벤트타입이 CallbackEventType.CONNECT_FAIL인 경우는 연결에 실패한 경우입니다.
콜백함수에서의 이벤트타입이 CallbackEventType.CONNECT_SUCCESS인 경우는 연결에 성공한 경우입니다. fromID와 TryConnect()시 반환된 sessionID를 비교해서 어떤 TryConnect()에 대한 이벤트인지를 구분할 수 있습니다.

If TryConnect() success, The listener will callback with CallbackEventType.ACCEPT_SUCCESS. Accepted SessionID is specified. Of cause You can send data to Accepted Session.

**You can send byte array to session with sessionID.**

Send()함수로 연결된 sessionID를 이용해서 세션에 바이트 배열을 전송할 수 있습니다:

        var bytes = new byte[100];
        // write data in bytes...
        
        ultraES.Send(sessionID, bytes);
        
If Send() success, data received session will execute callback with CallbackEventType.SESSION_RECEIVE_DATA. You can receive data by casting param object like this:

Send()가 성공하면 데이터를 수신한 세션은 콜백함수를 호출합니다. 이때 콜백함수 파라미터 fromID에는 데이터를 수신한 세션의 sessionID, param에는 수신한 바이트 배열값이 들어가 있습니다. param을 (byte[])으로 캐스팅하여 값을 얻어옵니다. 

        case CallbackEventType.SESSION_RECEIVE_DATA: // Session received data
                // fromID : sessionID that receive data
                // param : received byte array (byte[])
                var receivedByteArray = (byte[])param;
                // do something with receivedByteArray...
                break;
        
**UltraEasySocket's moto is clear: I send byte array and You receive same byte array. 
If I send you 100byte array, You will receive same 100byte array. Sent data will not be split or merged together.**

울트라이지소켓의 지향점은 명확합니다: 내가 보낸 바이트 배열을 수신하는 세션이 그대로 받는다는 것.
만약 100바이트 배열을 보냈다면 정확하게 똑같은 100바이트 배열을 받게 됩니다. 개별적으로 보낸 여러건의 데이터가 나눠지거나 합쳐져서 읽히지는 않습니다. TCP 소켓의 특징인 전송 데이터의 쪼개짐과 합쳐짐에 대한 처리는 모두 울트라이지소켓 내부에서 이루어 집니다. 사용자는 신경쓸 필요가 없습니다.

**If you want to disconnect session, Just Call CloseSession():**

연결된 세션을 끊고 싶으면 CloseSession()함수를 호출합니다.

        ultraES.CloseSession(sessionID);

This will close session. The session will execute callback with CallbackEventType.SESSION_CLOSED when socket closing completed. 
If some kind of socket error happen that session could not be maintained, or socket has disconnected by peer, session will execute callback with CallbackEventType.SESSION_CLOSED. 

CloseSession()을 호출한 후에 세션 종료처리가 완료되면 CallbackEventType.SESSION_CLOSED 이벤트로 콜백함수가 호출됩니다.
내가 직접 CloseSession()을 호출하지 않았다고 해도 상대방이 세션을 끊었거나, 소켓에 문제가 발생해서 끊겼다면 마찬가지로 CallbackEventType.SESSION_CLOSED 이벤트로 콜백함수가 호출됩니다.

**When exit program, You must call Terminate() function to clean up UltraEasyTcpSocket instance:**

프로그램 종료를 위해서는 UltraEasyTcpSocket의 Terminate() 함수를 호출해야 합니다. 이 함수를 호출하지 않으면 쓰레드가 남아있기 때문에 프로그램이 종료되지 않습니다.

        ultraES.Terminate();


**There are several sample projects included. Let me explain those projects one by one.**

라이브러리의 사용법을 보여주기 위해 여러개의 샘플 프로젝트가 포함되어 있습니다. 하나 하나 설명해 보겠습니다.

UltraEasySocket.Example is the project that shows basic usage of UltraEasySocket.Net. It creates one UltraEasyTcpSocket instance and start listening and tries connect. when connection established, connected session sends some string message. when session receives message, it disconnects session. You can see the flow of callback events.

UltraEasySocket.Example 프로젝트는 울트라이지소켓의 기본적이 작동을 보여주는 프로젝트입니다. 이 프로젝트는 리슨을 시작하고 연결을 시도하며, 연결되면 간단한 메시지를 전송합니다. 메시지를 수신한 세션은 메시지를 출력한 후 세션을 닫습니다. 그에따라 양측에 세션 닫힘 이벤트 콜백이 실행됩니다. 리스닝과 연결시도가 모두 같은 인스턴스에서 구현되었음에 유의하세요.

UltraEasySocket.ChatServerExample and UltraEasySocket.ChatClientExample are simple chat server and client.

UltraEasySocket.ChatServerExample 프로젝트와 UltraEasySocket.ChatClientExample 프로젝트는 울트라이지소켓을 이용하여 간단히 구현된 채팅 서버 / 클라이언트 예제입니다. 

UltraEasySocket.ExtremeTest is the project that doing burn-in test. It creates 1000 accept session and 1000 connect session. each session will give and take serial numeric messages, and some thread broadcasting text message to existing sessions. occasionally disconnects session and reconnect.

UltraEasySocket.ExtremeTest 프로젝트는 극한의 테스트를 통해 울트라이지소켓의 안정성을 보여주는 프로젝트입니다. 1000개의 accept 세션과 1000개의 connect 세션을 연결한 후 순차적으로 증가하는 메시지를 주고 받습니다. 이 와중에 별도의 쓰레드에서 스트링 메시지를 모든 세션에 전송합니다. 또한 가끔씩 세션을 고의로 접속 종료시키고 접속 종료가 완료되면 새 세션을 연결하는 것을 반복합니다. 이런 상황에서 잘못 읽히거나 누락된 메시지가 있는지 검사하고 세션의 수가 일정하게 유지되는지를 검사합니다.

**Message Encryption**

메시지 암호화 

UltraEasySocket has internal message encryption function. When create UltraEasyTcpSocket instnace, set parameter 'encryptLevel' 1 or 2. This will enable message encryption.

울트라소켓은 자체적으로 메시지 암호화 기능을 내장하고 있습니다. UltraEasyTcpSocket 인스턴스를 생성할때 encryptLevel 파라미터를 1 혹은 2로 설정하면 됩니다.

        var ultraES = new UltraEasyTcpSocket(new Action<CallbackEventType, long, object>(OnSocketEventCallback), encryptLevel:1);
        
encryptLevel 0(default) means no encryption. But It's speed is fastest.
encryptLevel 1 means xor based encryption. Balanced between speed and security.
encryptLevel 2 means AES-256 encryption. Slow but very secure.

encryptLevel 0(기본값)은 암호화하지 않는 상태이며 처리 속도가 가장 빠릅니다. 
encryptLevel 1은 xor 기반 암호화로 적당한 수준의 보안과 비교적 빠른 속도를 제공합니다.
encryptLevel 2는 AES-256 암호화로 속도는 느리지만 가장 안전한 통신 방법을 제공합니다. 

**Any question about this library send Email to sophnim@gmail.com**

라이브러리에 대한 질문이 있으시면 이메일을 보내주세요: sophnim@gmail.com

Thanks.

감사합니다.







