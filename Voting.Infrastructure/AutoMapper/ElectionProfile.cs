using AutoMapper;
using Voting.Infrastructure.API.Election;
using Voting.Infrastructure.DTO.Election;
using Voting.Model.Entities;

namespace Voting.Infrastructure.AutoMapper
{
    public class ElectionProfile : Profile
    {
        public ElectionProfile()
        {
            CreateMap<CreateElection, Election>();
            CreateMap<Election, ElectionDTO>();
            CreateMap<UpdateElection, Election>();

            CreateMap<CreateElectionCandidate, ElectionCandidate>();
            CreateMap<ElectionCandidate, ElectionCandidateDTO>()
                .ForMember(d => d.CandidateAddress, opt => opt.MapFrom(s => s.Candidate));
            CreateMap<UpdateElectionCandidate, ElectionCandidate>();
        }
    }
}