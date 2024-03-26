import { query } from './db.js'; // 수정된 부분

async function SkillQuery(){ 
    return await query(
`SELECT
JSON_OBJECT(
    'ID', skill.ID,
    'Name', skill.Name,
    'Explain', skill.\`Explain\`,
    'Cool Time', skill.\`Cool Time\`,
    'Status Int', (
        SELECT JSON_OBJECTAGG(status.\`Status Name\`, status.Value)
        FROM bearlike.skill_status status
        WHERE status.\`Skill ID\` = skill.ID AND status.\`Value Type\` = 0
    ),
    'Status Float', (
        SELECT JSON_OBJECTAGG(status.\`Status Name\`, status.Value)
        FROM bearlike.skill_status status
        WHERE status.\`Skill ID\` = skill.ID AND status.\`Value Type\` = 1
    )
) AS Skill
FROM bearlike.skill skill;`);
}

export async function MakeSkillData(app)
{
    var json = await SkillQuery();
    app.get('/Skill', async (req, res) => {
        try {
            res.json(json);
        } catch (error) {
            res.status(500).send('Skill Query error');
        }
    })
    json.forEach(data => {
        var skill = data.Skill; // 직접 Skill 객체에 접근
        var id = skill['ID'];
        app.get(`/Skill/${id}`, async (req,res) => {
            try {
                res.json(skill);
            } catch (error) {
                res.status(500).send('Skill Query error' + name);
            }
        })
    });
}