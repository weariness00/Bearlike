import { query, TableVesrionData } from './db.js'; // 수정된 부분

//#region Gun

async function GunInfoQuery()
{
    return await query(`SELECT * FROM bearlike.gun`);
}

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

async function MakeGunInfoData(app)
{
    app.get('/Gun/Version', async (req, res) => {
        var version = await TableVesrionData("Gun");
        res.json(version);
    });

    var json = await GunInfoQuery();
    app.get('/Gun', async (req, res) =>{ res.json(json); });
}

async function MakeGunStatusData(app)
{
    app.get('/Gun/Status/Version', async (req, res) => {
        var version = await TableVesrionData("Gun Status");
        res.json(version);
    });

    var json = await GunStatusQuery();
    app.get('/Gun/Status', async (req, res) => {
        try {
            res.json(json);
        } catch (error) {
            res.status(500).send('Gun Query error');
        }
    });

    json.forEach(data => {
        var id = data.ID;
        app.get(`/Gun/Status/${id}`, async (req,res) => {
            try {
                res.json(data);
            } catch (error) {
                res.status(500).send('Gun Query error' + id);
            }
        })
    });
}
//#endregion

export async function MakeData(app)
{
    MakeGunInfoData(app);
    MakeGunStatusData(app);
}