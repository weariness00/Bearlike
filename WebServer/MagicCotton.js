import { query, TableVesrionData } from './db.js'; // 수정된 부분

async function InfoQuery(){ 
    return await query(
        `SELECT
            mc.ID as "ID",
            mc.Name as "Name",
            MAX(mcs.Level) as "Max Level",
            COALESCE(
                (
                    SELECT JSON_OBJECTAGG(mcs.\`Level\`, mcs.\`Need Coin\`)
                    FROM bearlike.magic_cotton_status mcs
                    WHERE mcs.\`Cotton ID\` = mc.ID
                ),
                JSON_OBJECT()
            ) as 'Need Coin'
        FROM bearlike.magic_cotton mc
        JOIN bearlike.magic_cotton_status mcs ON mc.ID = mcs.\`Cotton ID\`
        GROUP BY mc.ID;`);
}

async function MakeInfoData(app)
{
    app.get('/MagicCotton/Version', async (req, res) => {
        var version = await TableVesrionData("Magic Cotton");
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

export async function MakeData(app)
{
    await MakeInfoData(app);
}