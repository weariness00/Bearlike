import { query, TableVesrionData } from './db.js'; // 수정된 부분

async function InfoQuery()
{
    return await query(`SELECT * FROM bearlike.skill;`)
}

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

async function MakeInfoData(app)
{
    app.get('/Skill/Version', async (req, res) => {
        var version = await TableVesrionData("Skill");
        res.json(version);
    })

    var json = await InfoQuery();
    app.get('/Skill', async (req, res) => { res.json(json); })
    json.forEach(data => {
        var id = data.ID;
        app.get(`/Skill/${id}`, async (req, res) => {
            res.json(data);
        });
    });
}

async function MakeStatusData(app)
{
    app.get('/Skill/Status/Version', async (req, res) => {
        var version = await TableVesrionData("Skill Status");
        res.json(version);
    })

    var json = await StatusQuery();
    app.get('/Skill/Status', async (req, res) => {
        try {
            res.json(json);
        } catch (error) {
            res.status(500).send('Skill Query error');
        }
    })
    json.forEach(data => {
        var id = data.ID;
        app.get(`/Skill/Status/${id}`, async (req,res) => {
            try {
                res.json(data);
            } catch (error) {
                res.status(500).send('Skill Query error' + name);
            }
        })
    });
}

export async function MakeData(app)
{
    await MakeInfoData(app);
    await MakeStatusData(app);
}