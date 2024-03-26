import { query } from './db.js'; // 수정된 부분

async function ItemQuery(){ 
    return await query(
`SELECT
JSON_OBJECT(
    'ID', item.ID,
    'Name', item.Name,
    'Status Int', COALESCE(
        (
            SELECT JSON_OBJECTAGG(status.\`Status Name\`, status.Value)
            FROM bearlike.item_status status
            WHERE status.\`Item ID\` = item.ID AND status.\`Value Type\` = 0
        ),
        JSON_OBJECT()
    ),
    'Status Float', COALESCE(
        (
            SELECT JSON_OBJECTAGG(status.\`Status Name\`, status.Value)
            FROM bearlike.item_status status
            WHERE status.\`Item ID\` = item.ID AND status.\`Value Type\` = 1
        ),
        JSON_OBJECT()
    )
) AS Item
FROM bearlike.item item;`);
}

export async function MakeItemData(app)
{
    var json = await ItemQuery();
    app.get('/Item', async (req, res) => {
        try {
            res.json(json);
        } catch (error) {
            res.status(500).send('Item Query error');
        }
    })
    json.forEach(data => {
        var item = data.Item; // 직접 Skill 객체에 접근
        var id = item['ID'];
        app.get(`/Item/${id}`, async (req,res) => {
            try {
                res.json(item);
            } catch (error) {
                res.status(500).send('Item Query error' + id);
            }
        })
    });
}