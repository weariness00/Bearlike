import { query, TableVesrionData } from './db.js'; // 수정된 부분

async function InfoQuery(){ return await query(`SELECT * FROM bearlike.item;`)};

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

async function MakeInfoData(app)
{
    app.get('/Item/Version', async (req, res) => {
        var version = await TableVesrionData("Item");
        res.json(version);
    })

    app.get('/Item/', async (req, res) => {
        var json = await InfoQuery();
        res.json(json);
    })

    app.get(`/Item/id/:id`, async (req,res) => {
        var id = req.params.id;
        var json = await InfoQuery();
        var data = json.find(i => i.ID == id);

        res.json(data);
    })

}
// 아이템 정보
async function MakeStatusData(app)
{
    app.get('/Item/Status/Version', async (req, res) => {
        var version = await TableVesrionData("Item Status");
        res.json(version);
    })

    app.get('/Item/Status/', async (req, res) => {
        var json = await StatusQuery();
        res.json(json);
    })
    
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