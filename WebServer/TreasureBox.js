import { query, TableVesrionData } from './db.js'; // 수정된 부분

// 모든 보물 상자 정보 테이블 생성
// - 상자 ID
// - 상자 설명 문자열
// - 드랍 조건
//  ㄴ 사용될 재화 Type (Coin, Magic Coin)
//  ㄴ 재화의 아이템 ID
//  ㄴ 필요한 재화 량
//  ㄴ 조건 설명
// - 루팅 테이블
//  ㄴ 아이템 Id
//  ㄴ 드랍 확률
//  ㄴ 드랍할 갯수
//  ㄴ 네트워크상의 아이템인지 개인 아이템인지
async function InfoQuery()
{
    return await query(
        `
SELECT 
    tb.ID as 'ID',
    tb.\`Explain\` as 'Explain',
    JSON_ARRAYAGG(
    JSON_OBJECT(
      'Open Condition Type', tc.\`Type\`,
      'Item ID', tc.\`Item ID\`,
      'Money Amount', tc.\`Money Amount\`,
      'Open Condition Explain', tc.Explain
    )
  ) as 'Condition',
    JSON_ARRAYAGG(
    JSON_OBJECT(
      'Item ID', tl.ItemID,
      'Probability', tl.Probability,
      'Amount', tl.Amount,
      'Is Networked', tl.\`Is Networked\` 
    )
  ) as 'LootingTable'
FROM 
    treasurebox_looting_table tl
JOIN 
    treasurebox_condition tc ON tl.\`Treasure Box ID\` = tc.\`Treasure Box ID\`
JOIN 
    treasurebox tb ON tl.\`Treasure Box ID\` = tb.ID 
JOIN 
    item i ON tl.ItemID = i.ID
GROUP BY tb.ID;
`
    );
}

// 모든 보물 상자 정보 테이블을 웹에 Json으로 기재
async function MakeInfoData(app)
{
    app.get('/TreasureBox/Version', async (req, res) => {
        var version = await TableVesrionData("TreasureBox");
        res.json(version);
    })

    app.get('/TreasureBox', async (req, res) => { 
        var json = await InfoQuery();
        res.json(json); 
    })
    app.get(`/TreasureBox/id/:id`, async (req, res) => {
        var id = req.params.id;
        var json = await InfoQuery();
        var data = json.find(s => s.ID == id);
        
        res.json(data);
    });
}

export async function MakeData(app)
{
    await MakeInfoData(app);
}