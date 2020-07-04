using System;

namespace CodeWarsRepoMaker
{
    class RepoMakerFactory
    {
        public IRepoMaker Create(Language language)
        {
            return language switch
            {
                Language.Ruby => new RubyRepoMaker(),
                Language.FSharp => new FSharpRepoMaker(),
                _ => throw new Exception("Unexpected Languaged passed to RepoMakerFactory.Create"),
            };
        }
    }
}
