import React, { useState } from 'react';
import nanoid from 'nanoid';
import { Grid, Form, Dropdown, Input, Button, Icon } from 'semantic-ui-react';

const ReturnCostsConstructor = ({ options, item, index, onChange, onAdd, onDelete, isAdd, t }) => {
    const [id] = useState(nanoid);

    return (
        <Grid.Row columns={3} key={id}>
            <Grid.Column>
                <Form.Field>
                    {/*<label>{t('orderNumber')}</label>*/}
                    <Dropdown
                        fluid
                        selection
                        options={options}
                        index={index}
                        name="id"
                        placeholder={t('orderNumber')}
                        value={item.id}
                        onChange={onChange}
                    />
                </Form.Field>
            </Grid.Column>
            <Grid.Column>
                <Form.Field>
                    {/*<label>{t('returnCostWithoutVAT')}</label>*/}
                    <Input
                        fluid
                        name="returnCostWithoutVAT"
                        value={item.returnCostWithoutVAT || ''}
                        index={index}
                        onChange={onChange}
                    />
                </Form.Field>
            </Grid.Column>
            <Grid.Column>
                {isAdd ? (
                    <Button icon onClick={onAdd}>
                        <Icon name="add" />
                    </Button>
                ) : null}
                <Button icon disabled={!(item.id || item.returnCostWithoutVAT)} onClick={() => onDelete(index)}>
                    <Icon name="trash alternate" />
                </Button>
            </Grid.Column>
        </Grid.Row>
    );
};

export default ReturnCostsConstructor;
