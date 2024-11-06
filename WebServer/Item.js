import { query, TableVesrionData } from './db.js'; // 수정된 부분

// 등록된 모든 아이템 테이블 가져오기
async function InfoQuery(){ return await query(`SELECT * FROM bearlike.item;`)};

// 등록된 모든 아이템의 특수 능력치 데이터 테이블 생성
// - 아이템 Name
// - 아이템 ID
// - Int형 능력치
//  ㄴ 능력치 Name
//  ㄴ 능력치 Value
// - Float형 능력치
//  ㄴ 능력치 Name
//  ㄴ 능력치 Value
async function StatusQuery(){ 
    return await query(
`SELECT
    item.ID as ID,
    item.Name as Name,
    item.\`Explain\` as 'Explain',
    COALESCE(
        (
            SELECT JSON_OBJECTAGG(status.\`Status Name\`, status.Value)
            FROM bearlike.item_status status
            WHERE status.\`Item ID\` = item.ID AND status.\`Value Type\` = 'Int'
        ),
        JSON_OBJECT()
    ) as 'Status Int',
    COALESCE(
        (
            SELECT JSON_OBJECTAGG(status.\`Status Name\`, status.Value)
            FROM bearlike.item_status status
            WHERE status.\`Item ID\` = item.ID AND status.\`Value Type\` = 'Float'
        ),
        JSON_OBJECT()
    ) as 'Status Float'
FROM bearlike.item item;`);
}

// 아이템 정보
async function MakeInfoData(app)
{
    // 현재 아이템 데이터 테이블의 버전 정보
    app.get('/Item/Version', async (req, res) => {
        var version = await TableVesrionData("Item");
        res.json(version);
    })

    // 모든 아이템 데이터 테이블
    app.get('/Item', async (req, res) => {
        var json = await InfoQuery();
        res.json(json);
    })

    // 특정 아이템 ID에 해당하는 데이터
    app.get(`/Item/id/:id`, async (req,res) => {
        var id = req.params.id;
        var json = await InfoQuery();
        var data = json.find(i => i.ID == id);

        res.json(data);
    })

}
async function MakeStatusData(app)
{
    // 특수 능력치 아이템 테이블의 버전 정보
    app.get('/Item/Status/Version', async (req, res) => {
        var version = await TableVesrionData("Item Status");
        res.json(version);
    })

    // 모든 아이템의 특수 능력치 데이터 테이블
    app.get('/Item/Status', async (req, res) => {
        var json = await StatusQuery();
        res.json(json);
    })
    
    // 특정 아이템의 ID에 해당하는 특수 능력치 데이터 
    app.get(`/Item/Status/id/:id`, async (req,res) => {
        var id = req.params.id;
        var json = await StatusQuery();
        var data = json.find(i => i.ID == id);
        res.json(data);
    })
}

export async function MakeData(app)
{
    await MakeInfoData(app);
    await MakeStatusData(app);
}