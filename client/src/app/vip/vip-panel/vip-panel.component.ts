import { Component, OnInit } from '@angular/core';
import { Member } from 'src/app/_models/member';
import { Pagination } from 'src/app/_models/pagination';
import { MembersService } from 'src/app/_services/members.service';

@Component({
  selector: 'app-vip-panel',
  templateUrl: './vip-panel.component.html',
  styleUrls: ['./vip-panel.component.css']
})
export class VipPanelComponent implements OnInit {
  members!: Partial<Member[]>;
  predicate='visited';
  pageNumber = 1;
  pageSize = 5;
  pagination!: Pagination;
  constructor(private memberService: MembersService) { }

  ngOnInit(): void {
    this.loadVisits();
  }

  loadVisits(){
    this.memberService.getVisits(this.predicate, this.pageNumber, this.pageSize).subscribe(response => {
      this.members = response.result;
      this.pagination= response.pagination;
    })
  }

  pageChanged(event: any){
    this.pageNumber = event.page;
    this.loadVisits();
  }
}
