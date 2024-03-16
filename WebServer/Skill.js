const { query } = require('./db');

async function SkillQuery(){ 
    return await query(
`SELECT
JSON_OBJECT(
    'Name', skill.Name,
    'Explain', skill.\`Explain\`,
    'Cool Time', skill.\`Cool Time\`,
    'Status Int', (
        SELECT JSON_OBJECTAGG(status.\`Status Name\`, status.Value)
        FROM bearlike.skill_status status
        WHERE status.\`Skill Name\` = skill.Name AND status.\`Value Type\` = 0
    ),
    'Status Float', (
        SELECT JSON_OBJECTAGG(status.\`Status Name\`, status.Value)
        FROM bearlike.skill_status status
        WHERE status.\`Skill Name\` = skill.Name AND status.\`Value Type\` = 1
    )
) AS Skill
FROM bearlike.Skill skill;`);
}


export async function MakeSkillData(app)
{
    var json = await SkillQuery();
    json.forEach(skill => {
        name = skill['Name'];
        app.get('/Skill/${name}', async (req,res) => {
            try {
                res.json(skill);
            } catch (error) {
                res.status(500).send('Skill Query error');
            }
        })
    });
}