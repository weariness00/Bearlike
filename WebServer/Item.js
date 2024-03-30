import { query, TableVesrionData } from './db.js'; // 수정된 부분

async function ItemInfoQuery(){ 
    return await query(
`SELECT
    item.ID as ID,
    item.Name as Name,
    item.\`Explain\` as 'Explain',
    COALESCE(
        (
            SELECT JSON_OBJECTAGG(status.\`Status Name\`, status.Value)
            FROM bearlike.item_status status
            WHERE status.\`Item ID\` = item.ID AND status.\`Value Type\` = 0
        ),
        JSON_OBJECT()
    ) as 'Status Int',
    COALESCE(
        (
            SELECT JSON_OBJECTAGG(status.\`Status Name\`, status.Value)
            FROM bearlike.item_status status
            WHERE status.\`Item ID\` = item.ID AND status.\`Value Type\` = 1
        ),
        JSON_OBJECT()
    ) as 'Status Float'
FROM bearlike.item item;`);
}

// 아이템 정보
async function MakeInfoData(app)
{
    app.get('/Item/Version', async (req, res) => {
        var version = await TableVesrionData("Item");
        res.json(version);
    })

    var json = await ItemInfoQuery();
    app.get('/Item/', async (req, res) => {
        try {
            res.json(json);
        } catch (error) {
            res.status(500).send('Item Query error');
        }
    })
    json.forEach(data => {
        var id = data.ID;
        app.get(`/Item/${id}`, async (req,res) => {
            try {
                res.json(data);
            } catch (error) {
                res.status(500).send('Item Query error' + id);
            }
        })
    });
}

export async function MakeData(app)
{
    await MakeInfoData(app);
}