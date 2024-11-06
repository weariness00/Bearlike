import { query, TableVesrionData } from './db.js'; // 수정된 부분

// 모든 스킬 정보 테이블
// - 스킬 Name
// - 스킬 Id
// - 스킬 설명
async function InfoQuery()
{
    return await query(`SELECT * FROM bearlike.skill;`)
}

// 모든 스킬 능력치 데이터 테이블 생성
// - 스킬 ID
// - 스킬 Name
// - Int형 능력치
//  ㄴ 능력치 Name
//  ㄴ 능력치 Value
// - Float형 능력치
//  ㄴ 능력치 Name
//  ㄴ 능력치 Value
async function StatusQuery(){ 
    return await query(
`SELECT
    skill.ID as ID,
    skill.Name as 'Skill Name',
    COALESCE(
        (
            SELECT JSON_OBJECTAGG(status.\`Status Name\`, status.Value)
            FROM bearlike.skill_status status
            WHERE status.\`Skill ID\` = skill.ID AND status.\`Value Type\` = 'Int'
        ),
        JSON_OBJECT()
    ) as 'Status Int',
    COALESCE(
        (
            SELECT JSON_OBJECTAGG(status.\`Status Name\`, status.Value)
            FROM bearlike.skill_status status
            WHERE status.\`Skill ID\` = skill.ID AND status.\`Value Type\` = 'Float'
        ),
        JSON_OBJECT()
    ) as 'Status Float'
FROM bearlike.skill skill;`);
}

// 모든 스킬 정보 텥이블 웹에 Json으로 기재
async function MakeInfoData(app)
{
    app.get('/Skill/Version', async (req, res) => {
        var version = await TableVesrionData("Skill");
        res.json(version);
    })

    app.get('/Skill', async (req, res) => { 
        var json = await InfoQuery();
        res.json(json); 
    })
    app.get(`/Skill/id/:id`, async (req, res) => {
        var id = req.params.id;
        var json = await InfoQuery();
        var data = json.find(s => s.ID == id);
        
        res.json(data);
    });
}

// 모든 스킬 능력치 테이블 웹에 Json으로 기재
async function MakeStatusData(app)
{
    app.get('/Skill/Status/Version', async (req, res) => {
        var version = await TableVesrionData("Skill Status");
        res.json(version);
    })

    app.get('/Skill/Status', async (req, res) => {
        var json = await StatusQuery();
        res.json(json);
    })
    
    app.get(`/Skill/Status/id/:id`, async (req,res) => {
        var id = req.params.id;
        var json = await StatusQuery();
        var data = json.find(s => s.ID == id);
        res.json(data);
    })
}

export async function MakeData(app)
{
    await MakeInfoData(app);
    await MakeStatusData(app);
}