import { query, TableVesrionData } from './db.js'; // 수정된 부분

async function InfoQuery()
{
    return await query(`SELECT * FROM bearlike.monster`);
}

// 모든 몬스터의 능력치 데이터 테이블 생성
// - 몬스터 ID
// - 몬스터 Name
// - Int형 능력치
//  ㄴ 능력치 Name
//  ㄴ 능력치 Value
// - Float형 능력치
//  ㄴ 능력치 Name
//  ㄴ 능력치 Value
async function MonsterStatusQuery(){ 
    return await query(
`SELECT
    monster.ID as ID,
    monster.Name as Name,
    COALESCE(
        (
            SELECT JSON_OBJECTAGG(status.\`Status Name\`, status.Value)
            FROM bearlike.monster_status status
            WHERE status.\`Monster ID\` = monster.ID AND status.\`Value Type\` = 'Int'
        ),
        JSON_OBJECT()
    ) as 'Status Int' ,
    COALESCE(
        (
            SELECT JSON_OBJECTAGG(status.\`Status Name\`, status.Value)
            FROM bearlike.monster_status status
            WHERE status.\`Monster ID\` = monster.ID AND status.\`Value Type\` = 'Float'
        ),  
        JSON_OBJECT()
    ) AS 'Status Float'
FROM bearlike.monster monster;`);
}


// 몬스터가 드랍하는 아이템에 대한 루팅 테이블 데이터 생성
// - 몬스터 ID
// - 몬스터 Name
// - 드랍할 아이템
//  ㄴ 아이템 Id
//  ㄴ 드랍 확률
//  ㄴ 드랍할 갯수
//  ㄴ 네트워크상의 아이템인지 개인 아이템인지
async function LootingTableQuery()
{
    return await query(
        `SELECT
        m.ID as ID,
        m.Name as MonsterName,
        JSON_ARRAYAGG(
          JSON_OBJECT(
            \'Item ID\', lt.\`Item ID\`,
            \'Probability\', lt.Probability,
            \'Amount\', lt.Amount,
            \'Is Networked\', lt.\`Is Networked\` 
          )
        ) as LootingTable
      FROM monster_looting_table lt
      JOIN monster m ON lt.\`Monster ID\`  = m.ID
      JOIN item i ON lt.\`Item ID\`  = i.ID
      GROUP BY lt.\`Monster ID\``
    );
}

// 모든 몬스터 정보 테이블 웹에 Json으로 기재
async function MakeInfoData(app)
{
    app.get('/Monster/Version', async (req, res) => {
        var version = await TableVesrionData("Monster");
        res.json(version);
    });

    app.get('/Monster', async (req, res) =>{ 
        var json = await InfoQuery();
        res.json(json); 
    });
}

// 모든 몬스터의 능력치 데이터 테이블 웹에 Json으로 기재
async function MakeStatusData(app)
{
    app.get('/Monster/Status/Version', async (req, res) => {
        var version = await TableVesrionData("Monster Status");
        res.json(version);
    });

    app.get('/Monster/Status', async (req, res) => {
        var json = await MonsterStatusQuery();
        res.json(json);
    });
    app.get(`/Monster/Status/id/:id`, async (req,res) => {
        var id = req.params.id;
        var json = await MonsterStatusQuery();
        var data = json.find(m => m.Id == id);
        res.json(data);
    })
}

// 모든 몬스터의 루팅 데이터 테이블 웹에 Json으로 기재
async function MakeLootingTable(app)
{
    app.get('/Monster/LootingTable/Version', async (req, res) => {
        var version = await TableVesrionData("Monster Looting Table");
        res.json(version);
    });

    app.get('/Monster/LootingTable', async (req, res) => {
        var json = await LootingTableQuery();
        res.json(json);
    });
    app.get(`/Monster/LootingTable/id/:id`, async (req,res) => {
        var id = req.params.id;
        var json = await LootingTableQuery();
        var data = json.find(m => m.ID == id);
        res.json(data);
    })
}

export async function MakeData(app)
{
    await MakeInfoData(app);
    await MakeStatusData(app);
    await MakeLootingTable(app);
}