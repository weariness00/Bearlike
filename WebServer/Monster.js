import { query } from './db.js'; // 수정된 부분

async function MonsterStatusQuery(){ 
    return await query(
`SELECT
    monster.ID as ID,
    monster.Name as Name,
    COALESCE(
        (
            SELECT JSON_OBJECTAGG(status.\`Status Name\`, status.Value)
            FROM bearlike.monster_status status
            WHERE status.\`Monster ID\` = monster.ID AND status.\`Value Type\` = 0
        ),
        JSON_OBJECT()
    ) as 'Status Int' ,
    COALESCE(
        (
            SELECT JSON_OBJECTAGG(status.\`Status Name\`, status.Value)
            FROM bearlike.monster_status status
            WHERE status.\`Monster ID\` = monster.ID AND status.\`Value Type\` = 1
        ),
        JSON_OBJECT()
    ) AS 'Status Float'
FROM bearlike.monster monster;`);
}

async function LootingTableQuery()
{
    return await query(
        `SELECT
        m.ID as MonsterID,
        m.Name as MonsterName,
        JSON_ARRAYAGG(
          JSON_OBJECT(
            \'Item ID\', lt.\`Item ID\`,
            \'Probability\', lt.Probability,
            \'Amount\', lt.Amount,
            \'Is Networked\', lt.\`Is Networked\` 
          )
        ) as LootingTable
      FROM monster_looting_table lt
      JOIN monster m ON lt.\`Monster ID\`  = m.ID
      JOIN item i ON lt.\`Item ID\`  = i.ID
      GROUP BY lt.\`Monster ID\``
    );
}

export async function MakeMonsterStatusData(app)
{
    var json = await MonsterStatusQuery();
    app.get('/Monster/Status', async (req, res) => {
        try {
            res.json(json);
        } catch (error) {
            res.status(500).send('Item Query error');
        }
    })
    json.forEach(data => {
        var id = data.ID;
        app.get(`/Monster/Status/${id}`, async (req,res) => {
            try {
                res.json(data);
            } catch (error) {
                res.status(500).send('Item Query error' + id);
            }
        })
    });
}

export async function MakeLootingTable(app)
{
    var json = await LootingTableQuery();
    app.get('/Monster/LootingTable', async (req, res) => {
        try {
            res.json(json);
        } catch (error) {
            res.status(500).send('Item Query error');
        }
    })  
    json.forEach(data => {
        var id = data.MonsterID;
        app.get(`/Monster/LootingTable/${id}`, async (req,res) => {
            try {
                res.json(data);
            } catch (error) {
                res.status(500).send('Item Query error' + id);
            }
        })
    });
}