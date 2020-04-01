using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace brainbeats_backend.Controllers
{
public class Sample
{
    public string id
    {
        get;
        set;
    }

    public string authorId
    {
        get;
        set;
    }

    public string name
    {
        get;
        set;
    }

    public bool? isPrivate
    {
        get;
        set;
    }

    public string file
    {
        get;
        set;
    }

    public string type
    {
        get;
        set;
    }
}
}
