const { query } = require('./db');

const texts = "ABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";

// DB의 Mathcing Room 정보를 불러오기
// 게임이 시작되지 않은 것만 불러오기
export function GetMatchingRoomDataNoStart() {
    var roomJson = query('SELECT * FROM bearlike.matching_room mr WHERE mr.`Is Game Start` = 0 and mr.`Player Count`< 3;');
    var roomDatas = roomJson.results.map(row => ({
        sessionName: row['Session Name'],
        playerCount: row['Player Count'],
        isGameStart: row['Game Start'] // assuming tinyint(1) is used as a boolean
    }));

    return roomDatas;
}

export function GetMatchingRoomDataAll() {
    var roomJson = query('SELECT * FROM bearlike.matching_room;');
    var roomDatas = roomJson.results.map(row => ({
        sessionName: row['Session Name'],
        playerCount: row['Player Count'],
        isGameStart: row['Game Start'] // assuming tinyint(1) is used as a boolean
    }));

    return roomDatas;
}

// DB에 Matching Room 정보를 추가
export function SetMatchingRoomQuery(sessionName, playerCount, isGameStart) {
    var e = query('INSERT INTO bearlike.matching_room VALUES (${seesionName}, ${playerCount}, ${isGameStart});');
    if (e == false) {
        return false;
    }
    else {
        return true;
    }
}

// Player Count가 0인 방을 제거
export function GCMathcingRoom() {
    query('DELETE FROM bearlike.matching_room WHERE `Player Count` = 0;');
}

// 해당 이름의 Session이 존재여부를 반환
export function CheckSessionName(sessionName) {
    var rows = query('SELECT EXISTS(SELECT 1 FROM bearlike.matching_room WHERE `Session Name` = ${sessionName})as IsExist;');
    const isExist = rows[0]['IsExist'];
    return isExist;
}

// 랜덤한 방에 매칭
export function RandomMatching() {
    var roomDatas = GetMatchingRoomData();
    if(roomDatas.length === 0)
    {
        MakeRoom();
        return;
    }
    var roomIndex = Math.floor(Math.random() * roomDatas.length);
    JoinRoom(roomDatas[roomIndex].seesionName);
    return roomDatas[roomIndex];
}

// 방 생성
export function MakeRoom() {
    var roomDatas = GetMatchingRoomData();
    var roomNames = roomDatas.map(room => room.sessionName);
    var sessionName = MakeSessionName(roomNames);
    SetMatchingRoomQuery(sessionName, 1, 0);
}

// 방 참가
export function JoinRoom(targetRoomName) {
    var roomDatas = GetMatchingRoomData();
    var roomNames = roomDatas.map(room => room.sessionName);
    for (let i = 0; i < roomDatas.length; i++) {
        var roomData = roomData[i];
        if(roomData.seesionName === targetRoomName)
        {
            SetMatchingRoomQuery(roomData.sessionName, roomData.playerCount + 1, 0);
            break;
        }
    }
}

function MakeSessionName(seesionNames) {
    let randomName;
    let isSuccess = false;
    while (true) {
        randomName = RandomString(6);
        isSuccess = true;
        for (const sessionName of sessionNames) {
            if (randomName === sessionName) {
                isSuccess = false;
                break;
            }
        }

        if (isSuccess) {
            break;
        }
    }

    return randomName;
}

function RandomString(length) {
    let result = '';
    const charactersLength = texts.length;
    for (let i = 0; i < length; i++) {
        result += text.charAt(Math.floor(Math.random() * charactersLength));
    }
    return result;
}