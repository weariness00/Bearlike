import { query } from './db.js'; // 수정된 부분

// async function MonsterStatusQuery(){ 
//     return await query(
// `SELECT
// JSON_OBJECT(
//     'ID', monster.ID,
//     'Name', monster.Name,
//     'Status Int', COALESCE(
//         (
//             SELECT JSON_OBJECTAGG(status.\`Status Name\`, status.Value)
//             FROM bearlike.monster_status status
//             WHERE status.\`Monster ID\` = monster.ID AND status.\`Value Type\` = 0
//         ),
//         JSON_OBJECT()
//     ),
//     'Status Float', COALESCE(
//         (
//             SELECT JSON_OBJECTAGG(status.\`Status Name\`, status.Value)
//             FROM bearlike.monster_status status
//             WHERE status.\`Monster ID\` = monster.ID AND status.\`Value Type\` = 1
//         ),
//         JSON_OBJECT()
//     )
// ) AS MonsterStatus
// FROM bearlike.monster monster;`);
// }

async function LootingTableQuery()
{
    return await query(
        `SELECT 
        s.ID  as ID,
        s.\`Type\` as Type,
        JSON_ARRAYAGG(
          JSON_OBJECT(
            \'Item ID\', lt.\`Item ID\`,
            \'Probability\', lt.Probability,
            \'Amount\', lt.Amount,
            \'Is Networked\', lt.\`Is Networked\` 
          )
        ) as LootingTable
      FROM stage_looting_table lt
      JOIN stage s ON lt.\`Stage ID\` = s.ID  
      JOIN item i ON lt.\`Item ID\`  = i.ID
      GROUP BY lt.\`Stage ID\`;`
    );
}

// export async function MakeMonsterStatusData(app)
// {
//     var json = await MonsterStatusQuery();
//     app.get('/Monster/Status', async (req, res) => {
//         try {
//             res.json(json);
//         } catch (error) {
//             res.status(500).send('Item Query error');
//         }
//     })
//     json.forEach(data => {
//         var monsterStatus = data.MonsterStatus; // 직접 Skill 객체에 접근
//         var id = monsterStatus['ID'];
//         app.get(`/Monster/Status/${id}`, async (req,res) => {
//             try {
//                 res.json(monsterStatus);
//             } catch (error) {
//                 res.status(500).send('Item Query error' + id);
//             }
//         })
//     });
// }

export async function MakeLootingTable(app)
{
    var json = await LootingTableQuery();
    app.get('/Stage/LootingTable', async (req, res) => {
        try {
            res.json(json);
        } catch (error) {
            res.status(500).send('Item Query error');
        }
    })  
    json.forEach(data => {
        var id = data.ID;
        app.get(`/Stage/LootingTable/${id}`, async (req,res) => {
            try {
                res.json(data);
            } catch (error) {
                res.status(500).send('Item Query error' + id);
            }
        })
    });
}