


// The following line can be removed. This removes the yellow input background in Chrome.
if(navigator.userAgent.toLowerCase().indexOf("chrome")>=0){$(window).load(function(){$("input:-webkit-autofill").each(function(){$(this).after(this.outerHTML).remove();$("input[name="+$(this).attr("name")+"]").val($(this).val())})})};
function getURLVar(key){
    var value=[],query=String(document.location).split('?');
    if(query[1]){
        var part=query[1].split('&');
        for(i=0; i<part.length; i++){
            var data=part[i].split('=');
            if(data[0]&&data[1]){
                value[data[0]]=data[1];
            }
        }
        if(value[key]){
            return value[key];
        } else{
            return '';
        }
    }
}

$(document).ready(function(){
    route=getURLVar('route');
    if(!route){
        $('#nav-home').addClass('active');
    }else{
        parts=route.split('/');
        if(parts[0]=='account'&&parts[1]!='wishlist'){
            $('#nav-account').addClass('active');
        }else{
            obj=$('#nav-'+parts[1]);
            if(obj.length){
                $(obj).addClass('active');
            }
        }
        $('.account-'+parts[1]).addClass('active');
        $('.affiliate-'+parts[1]).addClass('active');
    }
    $('.dropdown-menu').on('touchstart.dropdown.data-api',function(e){
        e.stopPropagation();
    });
    $('.dropdown-menu').on('click',function(e){
        e.stopPropagation();
    });
    if($(window).width()>=980){
        $('#nav-categories a[data-toggle="dropdown"]').click(function(){
            location.href=$(this).attr('href');
        });
    }
    $('.nav-collapse a[data-toggle]').click(function(){
        $(this).closest('.nav-collapse').css('height','100%');
    });
    $('[data-toggle="tooltip"]').tooltip();
    $('.modal').modalResponsiveFix({debug:true});
    $('.modal').touchScroll();
    $('.carousel-fade').carousel({pause:false});
    $('.nav-tabs a:first').tab('show');
    $('.help-block.error,.help-inline.error').closest('.control-group').addClass('error');
    $('#next-container').on('shown',function(){
        $(this).find(':not(#shipping).accordion-body.in').find('input:first').select();
    });
    $('#display .btn').click(function(){
        $.cookie('display',$(this).attr('id'));
        location.reload();
    });
    if($.cookie('display')){
        $('#'+$.cookie('display')).addClass('active');
    }else{
        $('#list').addClass('active');
    }
    $('[data-type="search"]').each(function(){
        var a=$(this);
        a.find('input[type="search"]').keyup(function(e){
            if(e.keyCode==13){
                searchFilter(a);
            }
        });
        a.find('.btn').click(function(e){
            searchFilter(a);
        });
    });
    $(document).on('click','a.colorbox',function(e){
        e.preventDefault();
        html ='<div class="modal hide fade" tabindex="-1" id="modal">';
        html+='<div class="modal-header"><a class="close" data-dismiss="modal">&times;</a><h3>'+$(this).attr('alt')+'</h3></div>';
        html+='<div class="modal-body" id="modal-body">';
        $.ajax(this.href,{async:false,success:function(data){
            html+=data;
        }});
        html+='</div>';
        html+='<div class="modal-footer"><a href="#" class="btn" data-dismiss="modal">Close</a></div>';
        html+='</div>';
        $('body').append(html);
        $('.modal').modalResponsiveFix({debug:true});
        $('#modal').modal();
    });

    $('#button-cart').bind('click',function(){
        btn=$(this);
        $.ajax({
            url:'index.php?route=checkout/cart/add',
            type:'post',
            data:$('#product-info').serialize(),
            dataType:'json',
            beforeSend:function(){
                btn.attr('disabled',true).after('<i class="icon-loading"></i>');
            },
            success:function(json){
                $('.help-inline.error').remove();
                $('.error').removeClass('error');
                $('button[disabled]').removeAttr('disabled');
                $('.icon-loading').remove();
                if(json['error']){
                    if(json['error']['option']){
                        for(i in json['error']['option']){
                            $('#option-'+i).append('<p class="help-inline error">'+json['error']['option'][i]+'</p>').closest('.control-group').addClass('error');
                        }
                    }
                }
                if(json['success']){
                    $('#notification').html('<div class="alert alert-success" style="display:none;"><a class="close" data-dismiss="alert" href="#">&times;</a>'+json['success']+'</div>');
                    $('.alert').fadeIn('slow').delay(15000).fadeTo(2000,0,function(){
                        $(this).remove();
                    });
                    $('#cart-total').html(json['total']);
                    $('#cart').load('index.php?route=module/cart #cart > *');
                }
            }
        });
        return false;
    });
    $(document).on('click','#review .pagination a',function(e){
        e.preventDefault();
        $('#review').fadeOut('slow');
        $('#review').load(this.href);
        $('#review').fadeIn('slow');
    });
    $('#review').load('index.php?route=product/product/review&product_id='+$('input[name="product_id"]').val());
});
function searchFilter(a){
    url=$('base').attr('href')+'index.php?route=product/search';
    var b=a.val();
    if(b){
        url+='&search='+encodeURIComponent(b);
    }
    var search=a.find('input[type="search"]').val();
    if(search){
        url+='&search='+encodeURIComponent(search);
    }
    var category_id=a.find('select[name="category_id"]').val();
    if(category_id>0){
        url+='&category_id='+encodeURIComponent(category_id);
    }
    var sub_category=a.find('input[name="sub_category"]:checked').val();
    if(sub_category){
        url+='&sub_category=true';
    }
    var description=a.find('input[name="description"]:checked').val();
    if(description){
        url+='&description=true';
    }
    location=url;
}
$(function(){
    $(document).on('focus','.date.datetime',function(){
        $(this).datetimepicker({
            todayBtn:1,
            autoclose:1,
            minView:0,
            showMeridian:1,
            format:'yyyy-mm-dd hh:mm',
            pickerPosition:'bottom-left'
        });
    });
    $(document).on('focus','.date.time',function(){
        $(this).datetimepicker({
            autoclose:1,
            startView:1,
            minView:0,
            maxView:1,
            showMeridian:1,
            format:"hh:ii",
            pickerPosition:'bottom-left'
        }).on('show',function(){
            $('.datetimepicker-hours thead,.datetimepicker-minutes thead').hide();
        }).on('hide',function(){
            $('.datetimepicker-hours thead,.datetimepicker-minutes thead').show();
        });
    });
    $(document).on('focus','.date',function(e){
        e.stopPropagation();
        $(this).datetimepicker({
            weekStart:1,
            todayBtn:1,
            autoclose:1,
            startView:2,
            minView:2,
            format:'yyyy-mm-dd',
            pickerPosition:'bottom-left'
        }).datetimepicker('show');
    });
});
function addReview(product_id,btn){
    $.ajax({
        url:'index.php?route=product/product/write&product_id='+product_id,
        type:'post',
        dataType:'json',
        data:'name='+encodeURIComponent($('input[name="name"]').val())+'&text='+encodeURIComponent($('textarea[name="text"]').val())+'&rating='+encodeURIComponent($('input[name="rating"]:checked').val()? $('input[name="rating"]:checked').val():'')+'&captcha='+encodeURIComponent($('input[name="captcha"]').val()),
        beforeSend:function(){
            $(btn).attr('disabled',true).after('<i class="icon-loading"></i>');
        },
        complete:function(){
            $(btn).removeAttr('disabled');
            $('.icon-loading').remove();
        },
        success:function(data){
            if(data['error']){
                $('#review-notification').html('<div class="alert alert-danger"><a class="close" data-dismiss="alert" href="#">&times;</a>'+data['error']+'</div>');
            }
            if(data['success']){
                $('#review-notification').html('<div class="alert alert-success"><a class="close" data-dismiss="alert" href="#">&times;</a>'+data['success']+'</div>');
                $('input[name="rating"]:checked').attr('checked','');
                $('input[name="name"],textarea[name="text"],input[name="captcha"]').val('');
            }
        }
    });
}



function addToCart(product_id,btn){
    $.ajax({
        url:'index.php?route=checkout/cart/add',
        type:'post',
        data:'product_id='+product_id+'&quantity=1',
        dataType:'json',
        beforeSend:function(){
            if(typeof btn!=='undefined'){
                $(btn).prop('disabled',true).after('<i class="icon-loading"></i>');
            }
        },
        complete:function(){
            $('button[disabled]').prop('disabled',false);
            $('.icon-loading').remove();
        },
        success:function(json){
            if(json['redirect']){
                location=json['redirect'];
            }
            if(json['success']){
                $('#notification').html('<div class="alert alert-success hide"><a class="close" data-dismiss="alert" href="#">&times;</a>'+json['success']+'</div>');
                $('.alert').fadeIn('slow').delay(15000).fadeTo(2000,0,function(){
                    $(this).remove();
                });
                $('#cart-total').html(json['total']);
                $('#cart').load('index.php?route=module/cart #cart > *');
            }
        }
    });
}





function addToWishList(product_id){
    $.ajax({
        url:'index.php?route=account/wishlist/add',
        type:'post',
        data:'product_id='+product_id,
        dataType:'json',
        success:function(json){
            if(json['success']){
                $('#notification').html('<div class="alert alert-info" style="display:none;"><a class="close" data-dismiss="alert" href="#">&times;</a>'+json['success']+'</div>');
                $('.alert').fadeIn('slow').delay(15000).fadeTo(2000,0,function(){
                    $(this).remove();
                });
                $('#wishlist-total').html(json['total']);
            }
        }
    });
}
function addToCompare(product_id){
    $.ajax({
        url:'index.php?route=product/compare/add',
        type:'post',
        data:'product_id='+product_id,
        dataType:'json',
        success:function(json){
            $('.alert').remove();
            if(json['success']){
                $('#notification').html('<div class="alert alert-info" style="display:none;"><a class="close" data-dismiss="alert" href="#">&times;</a>'+json['success']+'</div>');
                $('.alert').fadeIn('slow').delay(15000).fadeTo(2000,0,function(){
                    $(this).remove();
                });
                $('#compare-total').html(json['total']);
            }
        }
    });
}
// Affiliate
$(function(){
    $('input[name="payment"]').change(function(){
        $('.payment').hide();
        $('#payment-'+this.value).show();
    });
    $('input[name="payment"]:checked').change();
    var mapped={};
    $('input[name="product"]').typeahead({
        source:function(q,process){
            return $.getJSON('index.php?route=affiliate/tracking/autocomplete&filter_name='+encodeURIComponent(q),function(json){
                var data=[];
                $.each(json,function(i,item){
                    mapped[item.name]=item.link;
                    data.push(item.name);
                });
                process(data);
            });
        },
        updater:function(item){
            $('textarea[name="link"]').val(mapped[item]);
            return item;
        }
    });
});
$(function(){
    var $container=$('.thumbnails.product-masonry');
    $container.imagesLoaded(function(){
        $container.masonry({
            itemSelector:'li',
            isResizeBound:true
        });
    });
    $(".thumbnails.product-block").each(function(){
        var b=$(this);
        b.imagesLoaded(function(){
            b.find('li.thumbnail-grid').each(function(){
                $(this).removeClass('span2').css({width:($('img:first',this).width()+10)+'px'});
            })
            if($(window).width()>768){
                var c=-1;
                b.find('li').each(function(){
                    var d=$(this).height();
                    c=d>c?d:c;
                    $(this).css({
                        minHeight:c
                    })
                })
            }
        })
    })
});
$(document).ready(function(){
    $(document).on('change','select[name="customer_group_id"]',function(e){
        if(customer_group[this.value]){
            if(customer_group[this.value]['company_id_display']==1){
                $('#company-id-display').show();
            }else{
                $('#company-id-display').hide();
            }
            if(customer_group[this.value]['company_id_required']==1){
                $('#company-id-required').show();
            }else{
                $('#company-id-required').hide();
            }
            if(customer_group[this.value]['tax_id_display']==1){
                $('#tax-id-display').show();
            }else{
                $('#tax-id-display').hide();
            }
            if(customer_group[this.value]['tax_id_required']==1){
                $('#tax-id-required').show();
            }else{
                $('#tax-id-required').hide();
            }
        }
    });
    $('select[name="customer_group_id"]').change();
    var a=$('[data-provide="zone"]'),b=a.find('select[name="country_id"]');
    b.change(function(e){
        $.ajax({
            url:'index.php?route=account/register/country&country_id='+b.val(),
            dataType:'json',
            beforeSend:function(){
                b.after('<i class="icon-loading"></i>');
            },
            complete:function(){
                $('.icon-loading').remove();
            },
            success:function(json){
                if(json['postcode_required']==1){
                    a.find('#postcode-required').show();
                }else{
                    a.find('#postcode-required').hide();
                }
                if(json['zone']!=''){
                    html='<option value="">'+a.find('#text_select').val()+'</option>';
                    for(i=0;i<json['zone'].length;i++){
                        html+='<option value="'+json['zone'][i]['zone_id']+'"';
                        if(json['zone'][i]['zone_id']==a.find('#zone_id').val()){
                            html+=' selected=""';
                        }
                        html+='>'+json['zone'][i]['name']+'</option>';
                    }
                }else{
                    html='<option value="0" selected="">'+a.find('#text_none').val()+'</option>';
                }
                a.find('select[name="zone_id"]').html(html);
            },
            error:function(xhr,ajaxOptions,thrownError){
                alert(thrownError+"\r\n"+xhr.statusText+"\r\n"+xhr.responseText);
            }
        });
    });
    b.change();
});
// Twitter Widget
!function(d,s,id){var js,fjs=d.getElementsByTagName(s)[0];if(!d.getElementById(id)){js=d.createElement(s);js.id=id;js.src="https://platform.twitter.com/widgets.js";fjs.parentNode.insertBefore(js,fjs);}}(document,"script","twitter-wjs");
