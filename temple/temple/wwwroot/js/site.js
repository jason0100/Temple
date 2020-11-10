// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.






$(function () {
    let href = this.location.pathname;


    function showAlert(status, msg) {
        console.log('showAlert=' + status)
        color = 'alert-success';
        if (status !== 'True' && status !== true) {
            color = 'alert-danger';
            status = 'Error!';
        }
        else {
            status = 'Success!';
        }
        $.doTimeout('alert');
        $('#msgDiv').html('<div class="alert ' + color + ' alert-dismissible">' +
            '<button type="button" class="close" data-dismiss="alert">&times;</button>' +
            ' <strong>' + status + '</strong> ' + msg + '</div>');
        $("#msgDiv").show();
        $.doTimeout('alert', 4000, function () {
            $("#msgDiv").slideUp(200, function () {
                $(this).alert('close');
            });
        })

    }


    $('.item-delete-btn').click(function () {
        var id = $(this).data('id');
        //var id = $(this).attr('data-id');
        if (confirm('確認刪除此項目?' + id)) {

            $.get("Delete", { id: id }).done(function (data) {
                showAlert(data.isSuccess, data.message);
                //console.log('1data.isSuccess=' + data.isSuccess + "  " + data.message);
                if (data.isSuccess === true) {
                    $('#' + id).remove();
                }
            });
        }
    });

    $('#clearBtn').click(function () {
        $('input').val("");
    });

    $('#cancelBtn').click(function () {
        $('input:not([name="__RequestVerificationToken"]), select').val("");



    })

    var msg = $('#msg').val();
    var isSuccess = $('#isSuccess').val()
    //console.log("IsSuccess=" + msg);
    if (msg != '') {
        showAlert(isSuccess, msg);
    }

    let today = new Date();
    if (!href.includes("Login") && !href.includes("login") && !href.includes("Register") && !href.includes("register")) {
        $("#ReturnDateInput,#DueDateInput").datepicker({
            dateFormat: "yy-mm-dd",
            changeYear: true,
            yearRange: today.getFullYear() + ":" + parseInt(today.getFullYear() + 10),
            beforeShow: function () {
                setTimeout(function () {
                    $("#DueDateInput").attr("autocomplete", "off");
                }, 0);
            }
        });
    }

    let item_name = $("#FinancialItemId option:selected").text();
    if (item_name == "財利燈" || item_name == "光明燈") {
        $('#forLight').show();
    }
    else {
        $('#forLight').hide();

    }


    $("#FinancialItemId").on('change', function () {
        $('#forLight input, #forLight select').val("");
        let item_name = $('#FinancialItemId option:selected').text();

        if (item_name == "財利燈" || item_name == "光明燈") {
            $('#forLight').show();

        }
        else {
            $('#forLight').hide();
            $('#forLight input').val("");
        }
    })
    //for home/index todolist
    
    if (href == "/") {

        //編輯
        $('input[type="checkbox"]').on('change', function () {
            let checkbox = $(this);
            $(this).parents('li').toggleClass('completed');
            $(this).attr('disabled', true);
            let checked = $(this).prop('checked');
            $.post('/home/ChangeToDoListItem', { id: $(this).data('id'), IsDone: checked }).done(function (data) {
                console.log($(this))
                checkbox.attr('disabled', false);
                showAlert(data.isSuccess, data.message);
            });
        });


        //新增
        $('#add-task').click(function () {
            var post_data = { Subject: $('#new_task').val() };
            $.post('/home/CreateItem', post_data).done(function (data) {
                showAlert(data.isSuccess, data.message);
                if (data.isSuccess) {
                    $('#new_task').val('');
                    var item = JSON.parse(data.data);
                    console.log(item)
                    $('.todo-list-custom').append("<li><div class='form-check'><label class='form-check-label'><input class='checkbox' type='checkbox' data-id=" + item.id + "'/>" + item.subject + "<i class='input-helper'></i></label></div><i class='remove mdi mdi-close-circle-outline' data-id='" + item.id + "'></i></li>");
                }

            });
        });

        //刪除
        $('.remove').click(function () {
            let item = $(this).parent('li');
            //console.log($(this).siblings('.form-check').find('.checkbox').prop('disabled'));
            let checkbox = $(this).siblings('.form-check').find('.checkbox');
            let checkboxStatus = checkbox.prop('disabled');
            let checked = checkbox.prop('checked');
            //console.log('checked=' + checked)
            if (!checkboxStatus) {

                if (checked == true) {
                    //console.log($(this).data("id"));
                    var post_data = { id: $(this).data("id") };
                    $.post('/home/DeleteToDoListItem', post_data).done(function (data) {

                        showAlert(data.isSuccess, data.message);
                        if (data.isSuccess)
                            item.remove();
                    });
                }

            }
        });
    }

    //方位選擇
    $('#directionSelect').on('change', function () {

        $('#positionInput').val("");
        let direction = $('#directionSelect option:selected').val();
        var choose_direction;
        if (direction == "龍")
            choose_direction = "dragon";
        if (direction == "虎")
            choose_direction = "tiger";
        let lightType = $('#FinancialItemId option:selected').text();
        var data = {
            lightType: lightType
        };

        $.post('/FinancialRecordIncome/GetLightCount', data).done(function (data) {
            if (data.isSuccess) {
                console.log(data)
                var nextData = JSON.parse(data.data);
                for (var key in nextData) {
                    var value = nextData[key];
                    if (key.includes(choose_direction)) {
                        $('#positionInput').val(value);
                    }
                }
            }
        });
    });


    //uploadModal
    $('#inputGroupFile01').on('change', function () {
        var files = $('input[name="files"]').prop('files');//获取到文件列表
        var names = [];
        for (var i = 0; i != files.length; i++) {
            console.log(files[i].name)
            names.push(files[i].name);
        }
        if (files.length > 1) {
            $('#fileList').html(names.join("</br>"));
            $(this).next('label').html("multiple files");
        }
        else {
            $('#fileList').html("");
            $(this).next('label').html(names[0]);
        }

    })

    $('#uploadBtn').click(function () {
        var files = $('input[name="files"]').prop('files');//获取到文件列表
        if (files.length == 0) {
            alert('請選擇圖片');
            return;
        } else {
            var formData = new FormData();
            for (var i = 0; i != files.length; i++) {

                formData.append("files", files[i]);
            }
            $.ajax({
                url: "/Upload/UploadImage",
                data: formData,
                processData: false,
                contentType: false,
                type: "POST",
                success: function (data) {
                    //console.log(data);
                    alert(data.message);
                    if (data.isSuccess)
                        $('#exampleModal').modal('hide');
                }
            });
        }
    });

    if (!href.includes("Login") && !href.includes("login")) {
        //notify navbar
        setInterval(function () { GetNotifyForAjax() }, 3000);

        function GetNotifyForAjax() {
            $.post('/home/GetNotifyCountForAjax').done(function (data) {
                //console.log(data.data)
                $('.count-symbol').prop('hidden', false);
                if (data.data != 0) { $('.count-symbol').show() }
                else {
                    $('.count-symbol').hide();
                    $('#notifyData>a:not(:nth-last-child(1))').remove();
                    $('#setReadBtn').addClass('disabled')
                }
            });
        }
    }

    $('#notificationDropdown').click(function () {
        $.post('/home/GetNotifyDataForAjax').done(function (data) {
            var mydata = JSON.parse(data.data);
            //console.log(mydata)
            if (mydata != null) {
                $('#setReadBtn').removeClass('disabled')
                $('#notifyData>a:not(:nth-last-child(1))').remove();
                for (var item in mydata) {
                    var obj = mydata[item];
                    //console.log(obj)
                    let icon_tea = '<div class="preview-thumbnail"><div class="preview-icon bg-success"><i class="mdi mdi-leaf"></i></div></div>';
                    let icon_fortuneLight = '<div class="preview-thumbnail"><div class="preview-icon bg-warning"><i class="mdi mdi-coin"></i></div></div>';
                    let icon_brightLight = '<div class="preview-thumbnail"><div class="preview-icon bg-info"><i class="mdi mdi-white-balance-incandescent"></i></div></div>';
                    let icon_donate = '<div class="preview-thumbnail"><div class="preview-icon bg-danger"><i class="mdi mdi-cash-usd"></i></div></div>';
                    var icon;
                    switch (obj.itemName) {
                        case '青草茶': icon = icon_tea;
                            break;
                        case '光明燈': icon = icon_brightLight;
                            break;
                        case '財利燈': icon = icon_fortuneLight;
                            break;
                        case '捐贈': icon = icon_donate;
                            break;
                    }
                    let row = "<a href='/FinancialRecordIncome/GetIncome?Type=收入' class='dropdown-item preview-item'>" + icon + "<div class='preview-item-content d-flex align-items-start flex-column justify-content-center'>"
                        + "<h6 class='preview-subject font-weight-normal mb-1'>" + obj.memberName + "</h6><p class='text-gray ellipsis mb-0'>" + obj.itemName + "</p></div></a >"

                    $('#notifyData .dropdown-divider:eq(0)').after(row);
                }
            }
        });
    });


    //設為已讀
    $('#setReadBtn').click(function () {
        $.post('/home/ReadNotifyDataForAjax').done(function (data) {
            if (data.isSuccess) {
                showAlert(data.isSuccess, data.message);
            }

        });

    });

})
