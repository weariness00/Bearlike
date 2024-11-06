import { query, TableVesrionData } from './db.js'; // 수정된 부분

// 모든 Gun의 정보 테이블 생성
async function GunInfoQuery()
{
    return await query(`SELECT * FROM bearlike.gun`);
}

// 모든 Gun의 능력치 데이터 테이블 생성
// - Gun ID
// - Gun Name
// - Int형 능력치
//  ㄴ 능력치 Name
//  ㄴ 능력치 Value
// - Float형 능력치
//  ㄴ 능력치 Name
//  ㄴ 능력치 Value
async function GunStatusQuery()
{
    return await query(
        `SELECT
        gun.ID as ID,
        gun.Name as Name,
        COALESCE(
            (
                SELECT JSON_OBJECTAGG(status.\`Status Name\`, status.Value)
                FROM bearlike.gun_status status
                WHERE status.\`Gun ID\` = gun.ID AND status.\`Value Type\` = 'Int'
            ),
            JSON_OBJECT()
        ) as 'Status Int' ,
        COALESCE(
            (
                SELECT JSON_OBJECTAGG(status.\`Status Name\`, status.Value)
                FROM bearlike.gun_status status
                WHERE status.\`Gun ID\` = gun.ID AND status.\`Value Type\` = 'Float'
            ),
            JSON_OBJECT()
        ) AS 'Status Float'
        FROM bearlike.gun gun;`
    );
}

// 모든 Gun의 정보 테이블을 웹에 Json으로 기재
async function MakeGunInfoData(app)
{
    app.get('/Gun/Version', async (req, res) => {
        var version = await TableVesrionData("Gun");
        res.json(version);
    });
    
    app.get('/Gun', async (req, res) =>{ 
        var json = await GunInfoQuery();
        res.json(json); 
    }); 
}

// 모든 Gun의 능력치 데이터 테이블을 웹에 Json으로 기재
async function MakeGunStatusData(app)
{
    app.get('/Gun/Status/Version', async (req, res) => {
        var version = await TableVesrionData("Gun Status");
        res.json(version);
    });

    app.get('/Gun/Status', async (req, res) => {
        var json = await GunStatusQuery();
        try {
            res.json(json);
        } catch (error) {
            res.status(500).send('Gun Query error');
        }
    });

    app.get(`/Gun/Status/id/:id`, async (req,res) => {
        var id = req.params.id;
        var json = await GunStatusQuery();
        var data = json.find(g => g.ID == id)

        res.json(data);
    })
}

export async function MakeData(app)
{
    MakeGunInfoData(app);
    MakeGunStatusData(app);
}