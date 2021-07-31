# JmTunneler
## 소개
* JmTunneler는 SSH 터널링 프로그램 입니다.
* 방화벽을 우회하여 나의 포트를 상대 SSH에 바인드거나 상대 SSH에 접속하여 서비스 포트를 로컬에 바인드시키는 기능이 있습니다.

## 스크린샷
![image](https://user-images.githubusercontent.com/13088077/127727996-f56e6f13-deaf-43e6-a86a-fbfd0bfe477f.png)  
[Windows 서비스 기반 프로그램]

## 설정 방법
1. 압축을 서비스가 실행될 디렉터리에 풉니다.  
2. 아래와 같이 Seeting.ini 파일을 열어 수정합니다.  
``` ini
[Common]
SSH IP=example.com:2323
SSH ID=example
SSH PW=example123!@#
Type=S2C
List Interface=127.0.0.1
List Port=13389
Dest Host=127.0.0.1
Dest Port=3389
```
* S2C: 서버에 로컬포트 바인드  
* C2S: 로컬에 서버포트 바인드  
3. install.bat 파일을 관리자 권한으로 실행하여 서비스에 등록합니다.  
  
[추가]  
Windows 부팅 후 로그인을 안해도 자동으로 띄우고 싶으면 서비스 => JmTunneler 속성 => 로그온 => 사용자 계정 입력후 확인
