import React, {useState} from 'react';
import TextCropping from './TextCropping';
import {Button, Dropdown, Form, Grid, Icon, Loader, Modal, Popup} from 'semantic-ui-react';
import {useTranslation} from "react-i18next";
import Select from "../BaseComponents/Select";
import {changeCarrierRequest} from "../../ducks/autogrouping";
import {useDispatch} from "react-redux";

const TransportCompanies = ({ value, valueText, width, indexColumn,  alternativeCosts, source, runId, loadData, id}) => {
    const {t} = useTranslation();
    const dispatch = useDispatch();
    let [open, setOpen] = useState(false);
    let [formValue, setFormValue] = useState(null);

    const handleEdit = e => {
        e.stopPropagation();
        setOpen(true);
        setFormValue(value);
    };


    const handleChangeForm = (e, {value}) => {
        setFormValue(value);
    };

    const handleSave = () => {
        dispatch(changeCarrierRequest({
            params: {
                shippingId: id,
                carrierId: formValue
            },
            runId,
            callbackSuccess: () => {
                handleClose();
                loadData();
            }
        }))
    };

    const handleClose = () => {
        setFormValue(null);
        setOpen(false);
    };

    return (
        <>
            <TextCropping width={width} indexColumn={indexColumn}>
               {valueText}
            </TextCropping>
            <div>
                {value !== null ? (
                    <div style={{ display: 'flex' }}>
                        <div className="cell-grid-copy-btn">
                            <Icon name="pencil" size="small" onClick={handleEdit} />
                        </div>

                        <Popup
                            content={<Grid celled='internally'>
                                <Grid.Row>
                                    <Grid.Column textAlign="center">
                                        <h5>Альтернативная стоимость</h5>
                                    </Grid.Column>
                                </Grid.Row>
                                {
                                    alternativeCosts && alternativeCosts.map(item => (
                                        <Grid.Row key={item.carrierId} columns="equal">
                                            <Grid.Column>{item.carrierName}</Grid.Column>
                                            <Grid.Column>{item.cost}</Grid.Column>
                                        </Grid.Row>
                                    ))
                                }
                            </Grid>}
                            trigger={
                                <div className="cell-grid-info-btn">
                                    <Icon
                                        name="question"
                                        size="small"
                                        onClick={e => e.stopPropagation()}
                                    />
                                </div>
                            }
                            positionFixed
                            position="top center"
                        />
                    </div>
                ) : null}
            </div>
            <Modal open={open} size="tiny" onClick={e => e.stopPropagation()} onActionClick={e => e.stopPropagation()}>
                <Modal.Header>
                    Изменение транспортной компании
                </Modal.Header>
                <Modal.Content>
                    <Modal.Description>
                        <Form onSubmit={handleSave}>
                            <Form.Field>
                                <Dropdown
                                    fluid
                                    selection
                                    options={alternativeCosts && alternativeCosts.map(item => ({value: item.carrierId, text: item.carrierName, key: item.carrierId}))}
                                    value={formValue}
                                    onChange={handleChangeForm} />
                            </Form.Field>
                        </Form>
                    </Modal.Description>
                </Modal.Content>
                <Modal.Actions>
                    <Button onClick={handleClose}>{t('cancelConfirm')}</Button>
                    <Button
                        color="primary"
                        onClick={handleSave}
                    >
                        {t('SaveButton')}
                    </Button>
                </Modal.Actions>
            </Modal>
        </>
    );
};

export default TransportCompanies;
