# Signaling方法

## A -> Server

send connect

receive 「offer」

send 「offer」

## B -> Server

send connect

receive 「offer」

send 「answer」

send candidate if sever has A candidate

## A -> Server

receive 「answer」

# Candidateの交換方法

## A -> Server

send candidate
recieve candidate

## B -> Server

send candidate
recieve candidate


# 接続手順
- AがWebSocketでServerに接続
- BがWebSocketでServerに接続
- BとAで接続確立  
- CがWebSocketでServerに接続
- AがWebSocketでServerに接続
- CとAで接続確立
  - AがWebRTCで「join C」をBに送信
    - BがWebRTCでAを経由してCに「offer」を送信
    - CがWebRTCでAを経由してBに「answer」を送信
    - CとBで接続確立
      - BがWebRTCで「join C」をAに送信      
      - CがWebRTCで「join B」をAに送信
- DがWebSocketでServerに接続
- AがWebSocketでServerに接続
- DとAで接続確立
  - AがWebRTCで「join D」をBに送信
    - BがWebRTCでAを経由してDに「offer」を送信
    - DがWebRTCでAを経由してBに「answer」を送信
    - DとBで接続確立
      - BがWebRTCで「join D」をCに送信
      - BがWebRTCで「join D」をAに送信
      - DがWebRTCで「join B」をCに送信
      - DがWebRTCで「join B」をAに送信
  - AがWebRTCで「join D」をCに送信
    - CがWebRTCでAを経由してDに「offer」を送信
    - DがWebRTCでAを経由してCに「answer」を送信
    - DとCで接続確立
      - CがWebRTCで「join D」をAに送信
      - CがWebRTCで「join D」をBに送信
      - DがWebRTCで「join C」をAに送信
      - DがWebRTCで「join C」をBに送信

