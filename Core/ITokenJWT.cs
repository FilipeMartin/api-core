using taesa_aprovador_api.Models;

namespace taesa_aprovador_api.Core
{
    public interface ITokenJWT
    {
        object create(User user, string type);
    }
}