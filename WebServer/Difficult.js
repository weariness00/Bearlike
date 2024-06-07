import { query, TableVesrionData } from './db.js'; // 수정된 부분

async function DifficultQuery()
{
    return await query(`
    SELECT
        d.\`Difficult Name\` as Name,
        d.\`Explain\` as 'Explain',
        COALESCE(
            (
                SELECT JSON_OBJECTAGG(status.\`Status Name\`, status.Value)
                FROM difficult_status status
                WHERE status.\`Difficult Name\` = d.\`Difficult Name\` AND status.\`Value Type\` = 'Int'
            ),
            JSON_OBJECT()
        ) as 'Status Int',
        COALESCE(
            (
                SELECT JSON_OBJECTAGG(status.\`Status Name\`, status.Value)
                FROM difficult_status status
                WHERE status.\`Difficult Name\` = d.\`Difficult Name\` AND status.\`Value Type\` = 'Float'
            ),
            JSON_OBJECT()
        ) as 'Status Float'
    FROM difficult d;
    `);
}

async function MakeDifficultData(app)
{
    app.get('/Difficult/Version', async (req, res) => {
        var version = await TableVesrionData("Difficult");
        res.json(version);
    })

    app.get('/Difficult', async (req, res) => { 
        var json = await DifficultQuery();
        res.json(json); 
    })
    app.get(`/Difficult/name/:name`, async (req, res) => {
        var name = req.params.name;
        var json = await DifficultQuery();
        var data = json.find(d => d.Name.toLowerCase() == name);
        
        res.json(data);
    });
}

export async function MakeData(app)
{
    await MakeDifficultData(app);
}